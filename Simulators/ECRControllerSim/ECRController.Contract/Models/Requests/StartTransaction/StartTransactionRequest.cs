using Newtonsoft.Json;

namespace ECRController.Contract.Models.Requests.StartTransaction;

public class StartTransactionRequest
{
    [JsonProperty("transactionId")]
    public string TransactionId { get; set; }

    [JsonProperty("terminalId")]
    public string TerminalId { get; set; }

    [JsonProperty("transactionInformation")]
    public TransactionInformation TransactionInformation { get; set; }

    [JsonProperty("testCaseInformation")]
    public TestCaseInformation TestCaseInformation { get; set; }

    [JsonProperty("callbackUrl")]
    public string CallbackUrl { get; set; }
}