using ECRController.Contract.Models.Common;
using Newtonsoft.Json;

namespace ECRController.Contract.Models.Requests.StartTransaction;

public class TransactionInformation
{
    [JsonProperty("transactionType")]
    public TransactionType TransactionType { get; set; }

    [JsonProperty("amount")]
    public Amount Amount { get; set; }

    [JsonProperty("amountTip")]
    public Amount AmountTip { get; set; }

    [JsonProperty("amountOther")]
    public Amount AmountOther { get; set; }

    [JsonProperty("referenceNumber")]
    public string ReferenceNumber { get; set; }
}