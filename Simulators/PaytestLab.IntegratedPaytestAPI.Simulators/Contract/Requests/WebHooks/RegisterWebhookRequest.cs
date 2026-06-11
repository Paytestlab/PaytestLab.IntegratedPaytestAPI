using Newtonsoft.Json;
using PaytestLab.IntegratedPaytestAPI.Contract.Common;

namespace PaytestLab.IntegratedPaytestAPI.Contract.Requests.WebHooks;

public class RegisterWebhookRequest
{
    [JsonProperty("terminalId")]
    public string TerminalId { get; }

    [JsonProperty("event")]
    public EventType Event { get; }

    [JsonProperty("url")]
    public string Url { get; }

    public RegisterWebhookRequest(string terminalId, EventType @event, string url)
    {
        if (string.IsNullOrWhiteSpace(terminalId))
            throw new ArgumentException("Terminal ID cannot be null or empty.", nameof(terminalId));

        if (!Enum.IsDefined(typeof(EventType), @event))
            throw new ArgumentOutOfRangeException(nameof(@event), "Invalid event type specified.");

        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("URL cannot be null or empty.", nameof(url));
        if (!IsAbsoluteHttpUrl(url))
            throw new ArgumentException("URL must be an absolute HTTP or HTTPS URL.", nameof(url));

        TerminalId = terminalId;
        Event = @event;
        Url = url;
    }

    private static bool IsAbsoluteHttpUrl(string value)
        => Uri.TryCreate(value, UriKind.Absolute, out var uri)
           && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}