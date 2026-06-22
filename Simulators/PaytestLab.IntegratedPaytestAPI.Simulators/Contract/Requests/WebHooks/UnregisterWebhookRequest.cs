using Newtonsoft.Json;
using PaytestLab.IntegratedPaytestAPI.Contract.Common;

namespace PaytestLab.IntegratedPaytestAPI.Contract.Requests.WebHooks;

public class UnregisterWebhookRequest
{
    [JsonProperty("terminalId")]
    public string TerminalId { get; }

    [JsonProperty("event")]
    public EventType Event { get; }

    public UnregisterWebhookRequest(string terminalId, EventType eventType)
    {
        if (string.IsNullOrWhiteSpace(terminalId))
            throw new ArgumentException("Terminal ID cannot be null or empty.", nameof(terminalId));

        TerminalId = terminalId;
        Event = eventType;
    }
}