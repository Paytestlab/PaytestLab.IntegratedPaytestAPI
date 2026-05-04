using Newtonsoft.Json;

namespace ECRController.Contract.Models.Responses.Status;

public class TerminalStatus
{
    [JsonProperty("displayContent")]
    public string DisplayContent { get; set; }
}