using Newtonsoft.Json;

namespace PaytestLab.IntegratedPaytestAPI.Contract.Requests.StartTransaction;

public class AbortTransactionRequest
{
    [JsonProperty(nameof(TransactionId))]
    public string TransactionId { get; }

    [JsonProperty(nameof(SessionId))]
    public string SessionId { get; }

    public AbortTransactionRequest(string transactionId, string sessionId)
    {
        if (string.IsNullOrWhiteSpace(transactionId))
            throw new ArgumentException("Transaction ID cannot be null or empty.", nameof(transactionId));

        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("SessionId cannot be null or empty", nameof(sessionId));

        TransactionId = transactionId;
        SessionId = sessionId;
    }
}