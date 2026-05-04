using Newtonsoft.Json;

namespace ECRController.Contract.Models.Requests;

public class AbortTransactionRequest
{
    [JsonProperty("transactionId")]
    public string TransactionId { get; set; }
}