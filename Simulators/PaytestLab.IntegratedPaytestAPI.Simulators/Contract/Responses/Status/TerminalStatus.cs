using Newtonsoft.Json;

namespace PaytestLab.IntegratedPaytestAPI.Contract.Responses.Status;

public class TerminalStatus
{
    [JsonProperty(nameof(DisplayContent))]
    public string DisplayContent { get; set; }

    [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
    public TerminalState? Status { get; set; }
}