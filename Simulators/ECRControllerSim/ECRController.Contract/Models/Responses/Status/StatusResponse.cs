using Newtonsoft.Json;

namespace ECRController.Contract.Models.Responses.Status;

public class StatusResponse
{
    [JsonProperty("terminalId", Required = Required.Always)]
    public string TerminalId { get; set; }

    [JsonProperty("terminalStatus", Required = Required.Always)]
    public TerminalStatus TerminalStatus { get; set; }

    [JsonProperty("ECRStatus")]
    public ECRStatus ECRStatus { get; set; }
}