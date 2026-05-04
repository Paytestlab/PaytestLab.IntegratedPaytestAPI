using ECRController.Contract.Models.Common;
using Newtonsoft.Json;

namespace ECRController.Contract.Models.Responses.TransactionCompletion;

public class TransactionCompletionNotification
{
    [JsonProperty("transactionId", Required = Required.Always)]
    public string TransactionId { get; set; }

    [JsonProperty("transactionStatus", Required = Required.Always)]
    public string TransactionStatus { get; set; }

    [JsonProperty("authorizedAmount")]
    public Amount AuthorizedAmount { get; set; }

    [JsonProperty("tenderName")]
    public string TenderName { get; set; }

    [JsonProperty("posEntry")]
    public string PosEntry { get; set; }

    [JsonProperty("PANTruncated")]
    public string PANTruncated { get; set; }

    [JsonProperty("referenceNumber")]
    public string ReferenceNumber { get; set; }

    [JsonProperty("receiptMerchant")]
    public Receipt ReceiptMerchant { get; set; }

    [JsonProperty("receiptCardHolder")]
    public Receipt ReceiptCardHolder { get; set; }

    [JsonProperty("receiptCashRegister")]
    public Receipt ReceiptCashRegister { get; set; }

    [JsonProperty("hostResponse")]
    public HostResponse HostResponse { get; set; }
}