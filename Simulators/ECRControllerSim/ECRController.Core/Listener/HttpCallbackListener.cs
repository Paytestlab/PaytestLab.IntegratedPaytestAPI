using System.Net;
using System.Text;
using System.Threading.Channels;

namespace ECRController.Core.Listener;

public class HttpCallbackListener : ICallbackListener
{
    private const int DefaultLogBufferCapacity = 1000;

    private bool _disposed;

    private readonly HttpListener _listener = new();
    private readonly CancellationTokenSource _cts = new();
    private readonly Channel<string> _logChannel;

    private readonly Task _listenTask;
    private readonly Task _logConsumerTask;

    public event Func<string, string, Task> CallbackReceived;
    public event Func<string, Task> LogCallbackReceived;

    public HttpCallbackListener(string baseUrl, int logBufferCapacity = DefaultLogBufferCapacity)
    {
        _logChannel = Channel.CreateBounded<string>(new BoundedChannelOptions(logBufferCapacity)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true,
            SingleWriter = false
        });

        AddPrefixes(baseUrl);

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
        else
            _logChannel.Writer.TryWrite($"[unknown:{path}]\n{body}");
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

    private static async Task SafeInvokeAsync(Func<string, string, Task> del, string path, string body)
    {
        if (del == null) 
            return;

        try
        {
            await del(path, body).ConfigureAwait(false);
        }
        catch { /* ignore */ }
    }

    private static async Task SafeInvokeAsync( Func<string, Task> del, string logBody)
    {
        if (del == null) 
            return;

        try
        {
            await del(logBody).ConfigureAwait(false);
        }
        catch { /* ignore */ }
    }

    private void AddPrefixes(string baseUrl)
    {
        if (!baseUrl.EndsWith('/'))
            baseUrl += "/";

        _listener.Prefixes.Add(baseUrl + "transaction/");
        _listener.Prefixes.Add(baseUrl + "status/");

        // only for their debugging purposes
        _listener.Prefixes.Add(baseUrl);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed || !disposing)
            return;

        try
        {
            _cts.Cancel();
            _listener.Close();

            try
            {
                _listenTask.GetAwaiter().GetResult();
            }
            catch { /* ignore */ }

            try
            {
                _logConsumerTask.GetAwaiter().GetResult();
            }
            catch { /* ignore */ }
            _cts.Dispose();
        }
        finally
        {
            _disposed = true;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore().ConfigureAwait(false);

        Dispose(false);
        GC.SuppressFinalize(this);
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (_disposed)
            return;

        _cts.Cancel();
        _listener.Close();

        try
        {
            await _listenTask.ConfigureAwait(false);
        }
        catch { /* ignore */ }

        try
        {
            await _logConsumerTask.ConfigureAwait(false);
        }
        catch { /* ignore */ }

        _cts.Dispose();
        _disposed = true;
    }
}