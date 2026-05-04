using Newtonsoft.Json;

namespace ECRController.Contract.Models.Requests.Webhook;

public class WebhookRegistration
{
    [JsonProperty("terminalId")]
    public string TerminalId { get; set; }

    [JsonProperty("event")]
    public string Event { get; set; }

    [JsonProperty("url")]
    public string Url { get; set; }
}