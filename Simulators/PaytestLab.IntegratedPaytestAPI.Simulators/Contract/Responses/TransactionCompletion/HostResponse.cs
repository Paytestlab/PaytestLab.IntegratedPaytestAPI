using Newtonsoft.Json;

namespace PaytestLab.IntegratedPaytestAPI.Contract.Responses.TransactionCompletion;

public class HostResponse
{
    [JsonProperty(nameof(ResponseCode))]
    public string ResponseCode { get; set; }

    [JsonProperty(nameof(ResponseMessage))]
    public string ResponseMessage { get; set; }

    [JsonProperty(nameof(TransactionDateTime))]
    public string TransactionDateTime { get; set; }
}