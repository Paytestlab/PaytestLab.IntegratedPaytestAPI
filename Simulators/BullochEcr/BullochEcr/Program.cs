using ECRController.Contract.Models.Common;
using ECRController.Contract.Models.Requests;
using ECRController.Contract.Models.Requests.StartTransaction;
using ECRController.Contract.Models.Requests.Webhook;
using ECRController.Contract.Models.Responses.Status;
using ECRController.Contract.Models.Responses.TransactionCompletion;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Collections.Concurrent;
using System.Net;
using System.Text;
using JsonException = Newtonsoft.Json.JsonException;

namespace BullochServerSim;

internal class Program
{
    static readonly ConcurrentDictionary<string, string> _statusWebhooks = new();
    static readonly ConcurrentDictionary<string, string> _transactionWebhooks = new();
    static readonly ConcurrentDictionary<string, CancellationTokenSource> _activeTransactions = new();

    static HttpListener _listener;
    static readonly HttpClient _http = new(new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback
            = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    });

    static int _statusInterval;
    static int _transactionInterval;
    static readonly string _logFilePath = Path.Combine(AppContext.BaseDirectory, "logs.txt");

    static readonly JsonSerializerSettings _jsonSettings = new()
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
        MissingMemberHandling = MissingMemberHandling.Ignore,
        Converters = { new StringEnumConverter { AllowIntegerValues = true } }
    };

    static async Task Main()
    {
        var cfg = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", false, true)
            .Build();

        var host = cfg.GetValue<string>("Api:BaseUrl")!;
        var port = cfg.GetValue<int>("Api:Port");
        _statusInterval = cfg.GetValue<int>("StatusIntervalSeconds");
        _transactionInterval = cfg.GetValue<int>("TransactionIntervalSeconds");

        var prefix = $"{host}:{port}/";
        Log($"[SIM] Listening on {prefix}");
        Log($"[SIM] Status every {_statusInterval}s, Trx callback after {_transactionInterval}s\n");

        _listener = new HttpListener();
        _listener.Prefixes.Add(prefix);
        _listener.Start();

        _ = Task.Run(StatusBroadcastLoop);

        while (true)
        {
            var ctx = await _listener.GetContextAsync();
            _ = Task.Run(() => HandleRequest(ctx));
        }
    }

    static async Task HandleRequest(HttpListenerContext ctx)
    {
        try
        {
            var req = ctx.Request;
            var path = req.Url.AbsolutePath.TrimEnd('/');
            var method = req.HttpMethod;
            Log($"[REQ] {method} {path}");

            switch (path)
            {
                case "/transaction/start" when method == "POST": await OnStart(ctx); break;
                case "/transaction/abort" when method == "POST": await OnAbort(ctx); break;
                case "/status" when method == "GET": await OnStatus(ctx); break;
                case "/webhooks/register" when method == "POST": await OnRegister(ctx); break;
                case "/webhooks/unregister" when method == "POST": await OnUnregister(ctx); break;
                default:
                    ctx.Response.StatusCode = 404;
                    Write(ctx.Response, new { message = "Not found" });
                    break;
            }
        }
        catch (Exception ex)
        {
            Log($"[ERR] {ex}");
            ctx.Response.StatusCode = 500;
            Write(ctx.Response, new { message = "Internal simulator error" });
        }
        finally
        {
            ctx.Response.Close();
        }
    }

    static async Task OnStart(HttpListenerContext ctx)
    {
        var body = await ReadAndLogRequest(ctx);
        StartTransactionRequest req;
        try
        {
            req = JsonConvert.DeserializeObject<StartTransactionRequest>(body, _jsonSettings)!;
        }
        catch (JsonException ex)
        {
            Log($"[ERR] StartTransaction JSON: {ex.Message}");
            ctx.Response.StatusCode = 400;
            Write(ctx.Response, new { message = "Invalid JSON payload" });
            return;
        }

        if (string.IsNullOrWhiteSpace(req.TransactionId) ||
            string.IsNullOrWhiteSpace(req.CallbackUrl))
        {
            ctx.Response.StatusCode = 400;
            Write(ctx.Response, new { message = "transactionId or callbackUrl missing" });
            return;
        }

        var cts = new CancellationTokenSource();
        if (!_activeTransactions.TryAdd(req.TransactionId, cts))
        {
            ctx.Response.StatusCode = 409;
            Write(ctx.Response, new { message = "Transaction already active" });
            return;
        }

        _transactionWebhooks[req.TransactionId] = req.CallbackUrl!;
        ctx.Response.StatusCode = 200;
        Log($"[SIM] Transaction {req.TransactionId} started; callback in {_transactionInterval}s -> {req.CallbackUrl}");

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(_transactionInterval * 1000, cts.Token);

                var notif = new TransactionCompletionNotification
                {
                    TransactionId = req.TransactionId,
                    TransactionStatus = "Success",
                    AuthorizedAmount = new Amount { Value = 10.00, Currency = "EUR" },
                    TenderName = "Visa",
                    PosEntry = "Chip",
                    PANTruncated = "1234********5678",
                    ReferenceNumber = req.TransactionInformation?.ReferenceNumber ?? "",
                    ReceiptMerchant = new Receipt { Content = "Merchant receipt lines..." },
                    ReceiptCardHolder = new Receipt { Content = "Cardholder receipt lines..." },
                    ReceiptCashRegister = new Receipt { Content = "Cash register receipt..." },
                    HostResponse = new HostResponse
                    {
                        ResponseCode = "00",
                        ResponseMessage = "Approved",
                        TransactionDateTime = DateTime.UtcNow.ToString("o")
                    }
                };

                await PostJson(req.CallbackUrl!, notif, 204);
                Log($"[CBK] Sent completion for {req.TransactionId}");
            }
            catch (TaskCanceledException)
            {
                Log($"[SIM] Transaction {req.TransactionId} was aborted before completion.");
            }
            finally
            {
                if (_activeTransactions.TryRemove(req.TransactionId, out var toDispose))
                    toDispose.Dispose();
            }
        });
    }

    static async Task<string> ReadAndLogRequest(HttpListenerContext ctx)
    {
        var req = ctx.Request;
        Log("──── Incoming Request ────");
        Log($"{req.HttpMethod} {req.Url!.AbsolutePath}");
        Log("── Headers ──");
        foreach (var key in req.Headers.AllKeys)
            Log($"{key}: {req.Headers[key]}");
        string body = "";
        if (req.HasEntityBody)
        {
            using var reader = new StreamReader(req.InputStream, Encoding.UTF8);
            body = await reader.ReadToEndAsync();
            Log("── Body ──");
            Log(body);
        }
        Log("──────────────────────────");
        return body;
    }

    static async Task OnAbort(HttpListenerContext ctx)
    {
        var body = await new StreamReader(ctx.Request.InputStream, Encoding.UTF8).ReadToEndAsync();
        AbortTransactionRequest req;
        try
        {
            req = JsonConvert.DeserializeObject<AbortTransactionRequest>(body, _jsonSettings)!;
        }
        catch (JsonException ex)
        {
            Log($"[ERR] AbortTransaction JSON: {ex.Message}");
            ctx.Response.StatusCode = 400;
            Write(ctx.Response, new { message = "Invalid JSON payload" });
            return;
        }

        if (!_activeTransactions.TryRemove(req.TransactionId, out var cts))
        {
            ctx.Response.StatusCode = 404;
            Write(ctx.Response, new { message = "Transaction not found or already completed" });
            return;
        }

        cts.Cancel();
        cts.Dispose();
        ctx.Response.StatusCode = 204;
        Log($"[SIM] Transaction {req.TransactionId} aborted");
    }

    static Task OnStatus(HttpListenerContext ctx)
    {
        var term = ctx.Request.QueryString["terminalId"];
        if (string.IsNullOrWhiteSpace(term))
        {
            ctx.Response.StatusCode = 400;
            Write(ctx.Response, new { message = "terminalId missing" });
            return Task.CompletedTask;
        }

        var resp = new StatusResponse
        {
            TerminalId = term!,
            TerminalStatus = new TerminalStatus { DisplayContent = "Ready" },
            ECRStatus = new ECRStatus { DisplayContent = "OK" }
        };

        ctx.Response.StatusCode = 200;
        Write(ctx.Response, resp);
        Log($"[SIM] /status -> {term}");
        return Task.CompletedTask;
    }

    static async Task OnRegister(HttpListenerContext ctx)
    {
        var body = await new StreamReader(ctx.Request.InputStream, Encoding.UTF8).ReadToEndAsync();
        WebhookRegistration req;
        try
        {
            req = JsonConvert.DeserializeObject<WebhookRegistration>(body, _jsonSettings)!;
        }
        catch (JsonException ex)
        {
            Log($"[ERR] RegisterWebhook JSON: {ex.Message}");
            ctx.Response.StatusCode = 400;
            Write(ctx.Response, new { message = "Invalid JSON payload" });
            return;
        }

        if (req.Event != "status")
        {
            ctx.Response.StatusCode = 404;
            Write(ctx.Response, new { message = "Unknown event" });
            return;
        }

        if (!_statusWebhooks.TryAdd(req.TerminalId, req.Url!))
        {
            ctx.Response.StatusCode = 409;
            Write(ctx.Response, new { message = "Duplicate registration" });
            return;
        }

        ctx.Response.StatusCode = 201;
        Log($"[SIM] Registered status webhook: {req.TerminalId} -> {req.Url}");
    }

    static async Task OnUnregister(HttpListenerContext ctx)
    {
        var body = await new StreamReader(ctx.Request.InputStream, Encoding.UTF8).ReadToEndAsync();
        WebhookUnregistration req;
        try
        {
            req = JsonConvert.DeserializeObject<WebhookUnregistration>(body, _jsonSettings)!;
        }
        catch (JsonException ex)
        {
            Log($"[ERR] UnregisterWebhook JSON: {ex.Message}");
            ctx.Response.StatusCode = 400;
            Write(ctx.Response, new { message = "Invalid JSON payload" });
            return;
        }

        if (!_statusWebhooks.TryRemove(req.TerminalId, out _))
        {
            ctx.Response.StatusCode = 404;
            Write(ctx.Response, new { message = "No matching webhook" });
            return;
        }

        ctx.Response.StatusCode = 204;
        Log($"[SIM] Unregistered status webhook for {req.TerminalId}");
    }

    static async Task StatusBroadcastLoop()
    {
        while (true)
        {
            await Task.Delay(_statusInterval * 1000);
            foreach (var kv in _statusWebhooks)
            {
                var payload = new StatusResponse
                {
                    TerminalId = kv.Key,
                    TerminalStatus = new TerminalStatus { DisplayContent = "Auto-update" },
                    ECRStatus = new ECRStatus { DisplayContent = "OK" }
                };
                await PostJson(kv.Value, payload, 204);
                Log($"[CBK] Auto-status -> {kv.Value}");
            }
        }
    }

    static async Task PostJson(string url, object obj, int expect)
    {
        try
        {
            var payload = JsonConvert.SerializeObject(obj, _jsonSettings);
            using var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var resp = await _http.PostAsync(url, content);
            if ((int)resp.StatusCode != expect)
                Log($"[WARN] {url} returned {(int)resp.StatusCode}");
        }
        catch (Exception ex)
        {
            Log($"[ERR] posting to {url}: {ex.Message}");
        }
    }

    static void Write(HttpListenerResponse res, object obj)
    {
        res.ContentType = "application/json";
        var json = JsonConvert.SerializeObject(obj, _jsonSettings);
        using var w = new StreamWriter(res.OutputStream, Encoding.UTF8);
        w.Write(json);
    }

    static void Log(string message)
    {
        Console.WriteLine(message);
        try
        {
            File.AppendAllText(_logFilePath,
                message + Environment.NewLine);
        }
        catch { }
    }
}