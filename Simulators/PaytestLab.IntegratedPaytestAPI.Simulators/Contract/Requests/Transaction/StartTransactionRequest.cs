using Newtonsoft.Json;
using System.Transactions;

namespace PaytestLab.IntegratedPaytestAPI.Contract.Requests.StartTransaction;

public class StartTransactionRequest
{
    [JsonProperty(nameof(SessionId), Required = Required.Always)]
    public string SessionId { get; }

    [JsonProperty(nameof(TransactionId))]
    public string TransactionId { get; }

    [JsonProperty(nameof(TerminalId))]
    public string TerminalId { get; }

    [JsonProperty(nameof(TransactionInformation))]
    public TransactionInformation TransactionInformation { get; }

    [JsonProperty(nameof(CallbackUrl))]
    public string CallbackUrl { get; }

    public StartTransactionRequest(string sessionId, string transactionId, string terminalId, TransactionInformation transactionInformation, string callbackUrl)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("SessionId cannot be null or empty", nameof(sessionId));

        if (string.IsNullOrWhiteSpace(transactionId))
            throw new ArgumentException("TransactionId cannot be null or empty", nameof(transactionId));

        if (string.IsNullOrWhiteSpace(terminalId))
            throw new ArgumentException("TerminalId cannot be null or empty", nameof(terminalId));

        ArgumentNullException.ThrowIfNull(transactionInformation);

        if (string.IsNullOrWhiteSpace(callbackUrl))
            throw new ArgumentException("CallbackUrl cannot be null or empty", nameof(callbackUrl));
        if (!IsAbsoluteHttpUrl(callbackUrl))
            throw new ArgumentException("CallbackUrl must be an absolute HTTP or HTTPS URL", nameof(callbackUrl));

        SessionId = sessionId;
        TransactionId = transactionId;
        TerminalId = terminalId;
        TransactionInformation = transactionInformation;
        CallbackUrl = callbackUrl;
    }

    private static bool IsAbsoluteHttpUrl(string value)
        => Uri.TryCreate(value, UriKind.Absolute, out var uri)
           && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}