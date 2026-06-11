using Newtonsoft.Json;

namespace PaytestLab.IntegratedPaytestAPI.Contract.Responses.Status;

public class EcrStatus
{
    [JsonProperty(nameof(DisplayContent))]
    public string DisplayContent { get; set; }
}