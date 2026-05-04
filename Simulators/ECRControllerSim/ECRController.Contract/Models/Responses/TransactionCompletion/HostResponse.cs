using Newtonsoft.Json;

namespace ECRController.Contract.Models.Responses.TransactionCompletion;

public class HostResponse
{
    [JsonProperty("responseCode")]
    public string ResponseCode { get; set; }

    [JsonProperty("responseMessage")]
    public string ResponseMessage { get; set; }

    [JsonProperty("transactionDateTime")]
    public string TransactionDateTime { get; set; }
}
