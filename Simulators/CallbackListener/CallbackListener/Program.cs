using Microsoft.Extensions.Configuration;
using System.Net;
using System.Text;
using System.Threading.Channels;

namespace CallbackListenerDemo;

class Program
{
    // Log file path
    private static readonly string _logFile =
        Path.Combine(AppContext.BaseDirectory, "CallbackListener.log");

    static async Task Main(string[] args)
    {
        // Ensure log file exists and append
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_logFile)!);
            File.AppendAllText(_logFile, $"\n=== Session Started: {DateTime.Now:O} ===\n");
        }
        catch { /* ignore */ }

        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var baseUrl = config.GetValue<string>("Callback:BaseUrl");
        var capacity = config.GetValue<int>("Callback:LogBufferCapacity");

        using var listener = new HttpCallbackListener(baseUrl, capacity);

        Log($"Listening for callbacks on {baseUrl}/transaction/, {baseUrl}/status/");
        Log("Press Ctrl+C to exit.");

        listener.CallbackReceived += async (path, body) =>
        {
            Log($"[CALLBACK] {path}: {body}");
            await Task.CompletedTask;
        };

        listener.LogCallbackReceived += async body =>
        {
            Log($"[LOG CALLBACK] {body}");
            await Task.CompletedTask;
        };

        // Keep running until Ctrl+C
        var exit = new TaskCompletionSource<object>();
        Console.CancelKeyPress += (s, e) =>
        {
            e.Cancel = true;
            exit.TrySetResult(null);
        };
        await exit.Task;
    }

    // Writes to console and to log file
    static void Log(string message)
    {
        var line = $"[{DateTime.Now:HH:mm:ss}]: {message}";
        Console.WriteLine(line);
        try
        {
            File.AppendAllText(_logFile, line + Environment.NewLine);
        }
        catch
        {
            // ignore file errors
        }
    }
}

public class HttpCallbackListener : ICallbackListener
{
    private readonly HttpListener _listener = new();
    private readonly CancellationTokenSource _cts = new();
    private readonly Channel<string> _logChannel;

    private readonly Task _listenTask;
    private readonly Task _logConsumerTask;

    private bool _disposed;

    public event Func<string, string, Task> CallbackReceived;
    public event Func<string, Task> LogCallbackReceived;

    public HttpCallbackListener(string baseUrl, int logBufferCapacity = 1000)
    {
        var options = new BoundedChannelOptions(logBufferCapacity)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true,
            SingleWriter = false
        };
        _logChannel = Channel.CreateBounded<string>(options);

        if (!baseUrl.EndsWith('/'))
            baseUrl += "/";

        _listener.Prefixes.Add(baseUrl + "transaction/");
        _listener.Prefixes.Add(baseUrl + "status/");
        //_listener.Prefixes.Add(baseUrl + "logs/");


        // only for their debugging purposes
        var root = baseUrl.EndsWith('/') ? baseUrl : baseUrl + "/";
        _listener.Prefixes.Add(root);
        _listener.Prefixes.Add(root + "transaction/");
        _listener.Prefixes.Add(root + "status/");
        //_listener.Prefixes.Add(root + "logs/");

        _listener.Start();

        _listenTask = Task.Run(ListenLoopAsync);
        _logConsumerTask = Task.Run(LogConsumerAsync);
    }

    private async Task ListenLoopAsync()
    {
        using (_cts.Token.Register(() => _listener.Close()))
        {
            while (!_cts.Token.IsCancellationRequested && _listener.IsListening)
            {
                HttpListenerContext ctx;
                try
                {
                    ctx = await _listener.GetContextAsync().ConfigureAwait(false);
                }
                catch (HttpListenerException) { break; }
                catch (ObjectDisposedException) { break; }
                catch (OperationCanceledException) { break; }
                catch
                {
                    continue;
                }

                _ = ProcessRequestAsync(ctx);
            }
        }
    }

    private async Task ProcessRequestAsync(HttpListenerContext ctx)
    {
        string body;
        try
        {
            using var rdr = new StreamReader(ctx.Request.InputStream, Encoding.UTF8);
            body = await rdr.ReadToEndAsync().ConfigureAwait(false);
        }
        catch
        {
            ctx.Response.StatusCode = 500;
            ctx.Response.Close();
            return;
        }

        try
        {
            ctx.Response.StatusCode = 204;
            ctx.Response.Close();
        }
        catch { /* ignore */ }

        var path = ctx.Request.Url?.AbsolutePath.TrimEnd('/').ToLowerInvariant() ?? "";

        if (path.EndsWith("/transaction"))
        {
            await SafeInvokeAsync(CallbackReceived, "transaction", body).ConfigureAwait(false);
        }
        else if (path.EndsWith("/status"))
        {
            await SafeInvokeAsync(CallbackReceived, "status", body).ConfigureAwait(false);
        }
        else if (path.EndsWith("/logs"))
        {
            _logChannel.Writer.TryWrite(body);
        }
        else
        {
            _logChannel.Writer.TryWrite($"[unknown:{path}]\n{body}");
        }
    }

    private async Task LogConsumerAsync()
    {
        var reader = _logChannel.Reader;
        while (await reader.WaitToReadAsync().ConfigureAwait(false))
        {
            while (reader.TryRead(out var logBody))
            {
                await SafeInvokeAsync(LogCallbackReceived, logBody).ConfigureAwait(false);
            }
        }
    }

    private static async Task SafeInvokeAsync(Func<string, string, Task> del, string arg1, string arg2)
    {
        if (del == null)
            return;

        foreach (var handler in del.GetInvocationList().Cast<Func<string, string, Task>>())
        {
            try
            {
                await handler(arg1, arg2).ConfigureAwait(false);
            }
            catch { /* ignore */ }
        }
    }

    private static async Task SafeInvokeAsync(Func<string, Task> del, string arg)
    {
        if (del == null)
            return;

        foreach (var handler in del.GetInvocationList().Cast<Func<string, Task>>())
        {
            try
            {
                await handler(arg).ConfigureAwait(false);
            }
            catch { /* ignore */ }
        }
    }

    private async ValueTask DisposeAsyncCore()
    {
        _cts?.Cancel();
        _listener?.Close();

        _logChannel.Writer.Complete();
        if (_logConsumerTask is not null)
        {
            try
            {
                await _logConsumerTask.ConfigureAwait(false);
            }
            catch { /* ignore */ }
        }

        if (_listenTask is not null)
        {
            try
            {
                await _listenTask.ConfigureAwait(false);
            }
            catch { /* ignore */ }
        }

        _cts.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;
        try
        {
            await DisposeAsyncCore().ConfigureAwait(false);
        }
        catch
        { /* ignore */ }

        _disposed = true;
        GC.SuppressFinalize(this);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        try
        {
            DisposeAsyncCore().GetAwaiter().GetResult();
        }
        catch { /* ignore */ }

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}

public interface ICallbackListener : IAsyncDisposable, IDisposable
{
    event Func<string, string, Task> CallbackReceived;
    event Func<string, Task> LogCallbackReceived;
}