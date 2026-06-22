using Newtonsoft.Json;
using PaytestLab.IntegratedPaytestAPI.Contract.Common;
using System.Transactions;

namespace PaytestLab.IntegratedPaytestAPI.Contract.Responses.Session;

public class SessionTransactionSummary
{
    [JsonProperty(nameof(TransactionId), Required = Required.Always)]
    public string TransactionId { get; set; }

    [JsonProperty(nameof(TransactionStatus), Required = Required.Always)]
    public TransactionStatus TransactionStatus { get; set; }

    [JsonProperty(nameof(AuthorizedAmount))]
    public Amount AuthorizedAmount { get; set; }
}