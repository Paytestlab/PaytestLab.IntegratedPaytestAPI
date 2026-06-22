using Newtonsoft.Json;

namespace PaytestLab.IntegratedPaytestAPI.Contract.Responses.Status;

public class StatusResponse
{
    [JsonProperty(nameof(TerminalId), Required = Required.Always)]
    public string TerminalId { get; set; }

    [JsonProperty(nameof(TerminalStatus), Required = Required.Always)]
    public TerminalStatus TerminalStatus { get; set; }

    [JsonProperty(nameof(ECRStatus))]
    public EcrStatus ECRStatus { get; set; }
}