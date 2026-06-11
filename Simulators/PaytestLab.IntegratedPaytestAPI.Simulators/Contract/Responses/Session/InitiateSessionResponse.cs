using Newtonsoft.Json;

namespace PaytestLab.IntegratedPaytestAPI.Contract.Responses.Session;

public class InitiateSessionResponse
{
    [JsonProperty(nameof(SessionId), Required = Required.Always)]
    public string SessionId { get; set; }
}