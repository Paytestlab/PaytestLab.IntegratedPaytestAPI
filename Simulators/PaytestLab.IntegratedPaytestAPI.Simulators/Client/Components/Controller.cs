using Newtonsoft.Json;
using PaytestLab.IntegratedPaytestAPI.Client.Components.Client;
using PaytestLab.IntegratedPaytestAPI.Client.Components.Listener;
using PaytestLab.IntegratedPaytestAPI.Client.Components.Utils;
using PaytestLab.IntegratedPaytestAPI.Contract.Requests.Session;
using PaytestLab.IntegratedPaytestAPI.Contract.Requests.StartTransaction;
using PaytestLab.IntegratedPaytestAPI.Contract.Requests.WebHooks;
using PaytestLab.IntegratedPaytestAPI.Contract.Responses.Status;
using PaytestLab.IntegratedPaytestAPI.Contract.Responses.TransactionCompletion;
using System.Text;

namespace PaytestLab.IntegratedPaytestAPI.Client.Components;

public class Controller : IDisposable
{
    private readonly IClient _client;
    private readonly ICallbackListener _listener;
    private bool _disposed;

    public event Func<TransactionCompletionNotification, Task> TransactionCompleted = _ => Task.CompletedTask;
    public event Func<StatusResponse, Task> StatusUpdated = _ => Task.CompletedTask;
    public event Func<string, Task> LogNotifications = _ => Task.CompletedTask;

    public Controller(IClient client, ICallbackListener listener)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _listener = listener ?? throw new ArgumentNullException(nameof(listener));

        _listener.CallbackReceived += RouteCallback;
        _listener.LogCallbackReceived += RouteLog;
    }

    public Task<RequestResult> StartTransaction(StartTransactionRequest req, TestRunInformation info)
        => req is null || info is null
           ? Task.FromResult(RequestResult.Failure(0, "Request or TestRunInformation is null"))
           : _client.StartTransaction(req, info);

    public Task<RequestResult> AbortTransaction(AbortTransactionRequest req)
        => req is null
           ? Task.FromResult(RequestResult.Failure(0, "Request is null"))
           : _client.AbortTransaction(req);

    public Task<RequestResult<StatusResponse>> GetStatus(string terminalId)
        => string.IsNullOrWhiteSpace(terminalId)
           ? Task.FromResult(RequestResult<StatusResponse>.Failure(0, "terminalId cannot be empty"))
           : _client.GetStatus(terminalId);

    public Task<RequestResult> RegisterWebhook(RegisterWebhookRequest req)
        => req is null
           ? Task.FromResult(RequestResult.Failure(0, "Request is null"))
           : _client.RegisterWebhook(req);

    public Task<RequestResult> UnregisterWebhook(UnregisterWebhookRequest req)
        => req is null
           ? Task.FromResult(RequestResult.Failure(0, "Request is null"))
           : _client.UnregisterWebhook(req);

    public Task<RequestResult> InitiateSession(InitiateSessionRequest req)
    => req is null
       ? Task.FromResult(RequestResult.Failure(0, "Request is null"))
       : _client.InitiateSession(req);

    public Task<RequestResult> CompleteSession(CompleteSessionRequest req)
    => req is null
       ? Task.FromResult(RequestResult.Failure(0, "Request is null"))
       : _client.CompleteSession(req);

    private async Task RouteCallback(string path, string json)
    {
        try
        {
            switch (path)
            {
                case "transaction":
                    var txn = JsonConvert
                        .DeserializeObject<TransactionCompletionNotification>(json, JsonHelper.Settings);
                    if (txn is not null)
                        await TransactionCompleted(txn).ConfigureAwait(false);
                    break;

                case "status":
                    var st = JsonConvert
                        .DeserializeObject<StatusResponse>(json, JsonHelper.Settings);
                    if (st is not null)
                        await StatusUpdated(st).ConfigureAwait(false);
                    break;

                default:
                    await LogNotifications($"Unexpected callback path: '{path}'\nPayload: {json}")
                         .ConfigureAwait(false);
                    break;
            }
        }
        catch (Exception ex)
        {
            var errorMsg = new StringBuilder()
                .AppendLine($"Error handling callback '{path}':")
                .AppendLine($"  {ex.GetType().Name}: {ex.Message}")
                .AppendLine(ex.StackTrace ?? "")
                .ToString();

            await LogNotifications(errorMsg).ConfigureAwait(false);
        }
    }

    private async Task RouteLog(string json)
    {
        try
        {
            await LogNotifications(json).ConfigureAwait(false);
        }
        catch { /* ignore */}
    }

    public void Dispose()
    {
        if (_disposed)
            return;
        try
        {
            if (_listener is not null)
            {
                _listener.CallbackReceived -= RouteCallback;
                _listener.LogCallbackReceived -= RouteLog;
                _listener.Dispose();
            }
        }
        catch { /* ignore */ }

        try
        {
            _client?.Dispose();
        }
        catch { /* ignore */ }

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}