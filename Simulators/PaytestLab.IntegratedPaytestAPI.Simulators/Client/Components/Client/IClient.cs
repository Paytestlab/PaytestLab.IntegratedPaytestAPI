using PaytestLab.IntegratedPaytestAPI.Client.Components.Utils;
using PaytestLab.IntegratedPaytestAPI.Contract.Requests.Session;
using PaytestLab.IntegratedPaytestAPI.Contract.Requests.StartTransaction;
using PaytestLab.IntegratedPaytestAPI.Contract.Requests.WebHooks;
using PaytestLab.IntegratedPaytestAPI.Contract.Responses.Status;

namespace PaytestLab.IntegratedPaytestAPI.Client.Components.Client;

public interface IClient : IDisposable
{
    public Uri Uri { get; }

    Task<RequestResult> InitiateSession(InitiateSessionRequest request, CancellationToken ct = default);
    Task<RequestResult> CompleteSession(CompleteSessionRequest request, CancellationToken ct = default);
    Task<RequestResult> StartTransaction(StartTransactionRequest request, TestRunInformation testRunInformation, CancellationToken ct = default);
    Task<RequestResult> AbortTransaction(AbortTransactionRequest request, CancellationToken ct = default);
    Task<RequestResult<StatusResponse>> GetStatus(string terminalId, CancellationToken ct = default);
    Task<RequestResult> RegisterWebhook(RegisterWebhookRequest request, CancellationToken ct = default);
    Task<RequestResult> UnregisterWebhook(UnregisterWebhookRequest request, CancellationToken ct = default);
}