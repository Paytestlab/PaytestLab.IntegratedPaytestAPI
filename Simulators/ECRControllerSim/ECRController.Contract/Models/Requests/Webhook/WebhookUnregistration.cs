using Newtonsoft.Json;

namespace ECRController.Contract.Models.Requests.Webhook;

public class WebhookUnregistration
{
    [JsonProperty("terminalId")]
    public string TerminalId { get; set; }

    [JsonProperty("event")]
    public string Event { get; set; }
}