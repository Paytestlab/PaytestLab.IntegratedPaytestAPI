using ECRController.Contract.Models.Requests;
using ECRController.Contract.Models.Requests.StartTransaction;
using ECRController.Contract.Models.Requests.Webhook;
using ECRController.Contract.Models.Responses.Status;
using ECRController.Core.Utils;

namespace ECRController.Core.Client;

public interface IEcrClient : IDisposable
{
    Task<RequestResult> StartTransaction(StartTransactionRequest request, TestRunInformation testRunInformation);
    Task<RequestResult> AbortTransaction(AbortTransactionRequest request);
    Task<RequestResult<StatusResponse>> GetStatus(string terminalId);
    Task<RequestResult> RegisterWebhook(WebhookRegistration request);
    Task<RequestResult> UnregisterWebhook(WebhookUnregistration request);
}