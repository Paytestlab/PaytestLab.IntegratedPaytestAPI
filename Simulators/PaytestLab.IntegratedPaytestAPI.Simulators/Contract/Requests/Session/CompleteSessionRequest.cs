using Newtonsoft.Json;

namespace PaytestLab.IntegratedPaytestAPI.Contract.Requests.Session;

public class CompleteSessionRequest
{
    [JsonProperty("sessionId", Required = Required.Always)]
    public string SessionId { get; }

    public CompleteSessionRequest(string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("SessionId cannot be null or empty", nameof(sessionId));

        SessionId = sessionId;
    }
}