using Newtonsoft.Json;

namespace ECRController.Contract.Models.Responses.Status;

public class ECRStatus
{
    [JsonProperty("displayContent")]
    public string DisplayContent { get; set; }
}