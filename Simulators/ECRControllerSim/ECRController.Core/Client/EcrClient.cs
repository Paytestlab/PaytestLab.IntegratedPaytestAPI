using ECRController.Contract.Models.Requests;
using ECRController.Contract.Models.Requests.StartTransaction;
using ECRController.Contract.Models.Requests.Webhook;
using ECRController.Contract.Models.Responses.Status;
using ECRController.Core.Utils;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace ECRController.Core.Client;

public class EcrClient : IEcrClient
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };
    private bool disposedValue;

    public EcrClient(HttpClient http, Uri baseUri)
    {
        if (http == null || baseUri == null)
            throw new ArgumentNullException("HttpClient and BaseUri must be provided");

        _http = http;
        _http.BaseAddress = baseUri;
        _http.DefaultRequestHeaders.Accept
             .Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
    }

    public Task<RequestResult> StartTransaction( StartTransactionRequest request, TestRunInformation testRunInformation)
    {
        if (request == null)
            return Fail("request is null");
        if (testRunInformation == null)
            return Fail("testRunInformation is null");

        var msg = new HttpRequestMessage(HttpMethod.Post, ApiPaths.TransactionStart)
        {
            Content = JsonContent.Create(request, options: _jsonOptions)
        };

        msg.Headers.Add("X-TestRun-Id", testRunInformation.Id);
        msg.Headers.Add("X-TestCase-Id", testRunInformation.TestCaseId);

        return SendAsync(() => _http.SendAsync(msg));
    }

    public Task<RequestResult> AbortTransaction(AbortTransactionRequest req)
    {
        if (req == null)
            return Fail("req is null");

        return PostAsync(ApiPaths.TransactionAbort, req);
    }

    public Task<RequestResult<StatusResponse>> GetStatus(string terminalId)
    {
        if (string.IsNullOrWhiteSpace(terminalId))
            return Fail<StatusResponse>("terminalId cannot be empty");

        return GetAsync<StatusResponse>(ApiPaths.StatusWithTerminal(terminalId));
    }

    public Task<RequestResult> RegisterWebhook(WebhookRegistration req)
    {
        if (req == null)
            return Fail("req is null");

        return PostAsync(ApiPaths.WebhooksRegister, req);
    }

    public Task<RequestResult> UnregisterWebhook(WebhookUnregistration req)
    {
        if (req == null)
            return Fail("req is null");

        return PostAsync(ApiPaths.WebhooksUnregister, req);
    }

    // ——— Helpers ———

    private Task<RequestResult> PostAsync<T>(string uri, T content) => SendAsync(() => _http.PostAsJsonAsync(uri, content, _jsonOptions));

    private Task<RequestResult<TResponse>> GetAsync<TResponse>(string uri) => SendAsync<TResponse>(() => _http.GetAsync(uri));

    private static async Task<RequestResult> SendAsync(Func<Task<HttpResponseMessage>> action)
    {
        var generic = await SendAsync<object>(action, hasPayload: false).ConfigureAwait(false);

        return generic.IsSuccess ? RequestResult.Success(generic.Status, generic.Content) : RequestResult.Failure(generic.Status, generic.Content);
    }

    private static async Task<RequestResult<T>> SendAsync<T>(Func<Task<HttpResponseMessage>> action, bool hasPayload = true)
    {
        try
        {
            using var resp = await action().ConfigureAwait(false);
            var body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (!resp.IsSuccessStatusCode)
                return RequestResult<T>.Failure(resp.StatusCode, body);

            if (!hasPayload)
                return RequestResult<T>.Success(resp.StatusCode, default!, body);

            var data = JsonSerializer.Deserialize<T>(body, _jsonOptions)!;
            return RequestResult<T>.Success(resp.StatusCode, data, body);
        }
        catch (HttpRequestException ex)
        {
            return RequestResult<T>.Failure(HttpStatusCode.ServiceUnavailable, $"Network error: {ex.Message}");
        }
        catch (Exception ex)
        {
            return RequestResult<T>.Failure(HttpStatusCode.InternalServerError, $"Unexpected error: {ex.Message}");
        }
    }

    private static Task<RequestResult> Fail(string message) => Task.FromResult(RequestResult.Failure(0, message));

    private static Task<RequestResult<T>> Fail<T>(string message) => Task.FromResult(RequestResult<T>.Failure(0, message));

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _http?.Dispose();
            }
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}