using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PaytestLab.IntegratedPaytestAPI.Contract.Common;
using System.Transactions;

namespace PaytestLab.IntegratedPaytestAPI.Contract.Responses.TransactionCompletion;

public class TransactionCompletionNotification
{
    [JsonProperty(nameof(TransactionId), Required = Required.Always)]
    public string TransactionId { get; set; }

    [JsonProperty(nameof(TransactionStatus), Required = Required.Always)]
    public TransactionStatus TransactionStatus { get; set; }

    [JsonProperty(nameof(AuthorizedAmount))]
    public Amount AuthorizedAmount { get; set; }

    [JsonProperty(nameof(TenderName))]
    public string TenderName { get; set; }

    [JsonProperty(nameof(PosEntry))]
    public string PosEntry { get; set; }

    [JsonProperty(nameof(PANTruncated))]
    public string PANTruncated { get; set; }

    [JsonProperty(nameof(ReferenceNumber))]
    public string ReferenceNumber { get; set; }

    [JsonProperty(nameof(ReceiptMerchant))]
    public Receipt ReceiptMerchant { get; set; }

    [JsonProperty(nameof(ReceiptCardHolder))]
    public Receipt ReceiptCardHolder { get; set; }

    [JsonProperty(nameof(ReceiptCashRegister))]
    public Receipt ReceiptCashRegister { get; set; }

    [JsonProperty(nameof(HostResponse))]
    public HostResponse HostResponse { get; set; }

    [JsonProperty("iccData", NullValueHandling = NullValueHandling.Ignore)]
    public JObject IccData { get; set; }
}