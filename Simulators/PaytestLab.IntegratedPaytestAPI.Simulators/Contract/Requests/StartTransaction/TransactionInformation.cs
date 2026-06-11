using Newtonsoft.Json;
using PaytestLab.IntegratedPaytestAPI.Contract.Common;

namespace PaytestLab.IntegratedPaytestAPI.Contract.Requests.StartTransaction;

public class TransactionInformation
{
    [JsonProperty("transactionType")]
    public TransactionType TransactionType { get; }

    [JsonProperty("amount")]
    public Amount Amount { get; }

    [JsonProperty("amountTip")]
    public Amount AmountTip { get; set; }

    [JsonProperty("amountOther")]
    public Amount AmountOther { get; set; }

    [JsonProperty("referenceNumber")]
    public string ReferenceNumber { get; set; }

    public TransactionInformation(TransactionType transactionType, Amount amount)
    {
        if (!Enum.IsDefined(typeof(TransactionType), transactionType))
            throw new ArgumentOutOfRangeException(nameof(transactionType), "Invalid transaction type specified.");

        TransactionType = transactionType;
        Amount = amount ?? throw new ArgumentNullException(nameof(amount));
    }
}