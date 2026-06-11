using Newtonsoft.Json;

namespace PaytestLab.IntegratedPaytestAPI.Contract.Responses;

public class ErrorResponse
{
    [JsonProperty(nameof(Message), Required = Required.Always)]
    public string Message { get; set; }
}