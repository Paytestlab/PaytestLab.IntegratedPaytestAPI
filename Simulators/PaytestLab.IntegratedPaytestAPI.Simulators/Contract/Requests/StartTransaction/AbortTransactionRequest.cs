using Newtonsoft.Json;

namespace PaytestLab.IntegratedPaytestAPI.Contract.Requests.StartTransaction;

public class AbortTransactionRequest
{
    [JsonProperty(nameof(TransactionId))]
    public string TransactionId { get; }

    public AbortTransactionRequest(string transactionId)
    {
        if (string.IsNullOrWhiteSpace(transactionId))
            throw new ArgumentException("Transaction ID cannot be null or empty.", nameof(transactionId));

        TransactionId = transactionId;
    }
}