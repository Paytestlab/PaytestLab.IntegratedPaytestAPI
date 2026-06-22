using Newtonsoft.Json;

namespace PaytestLab.IntegratedPaytestAPI.Contract.Requests.Session;

public class InitiateSessionRequest
{
    [JsonProperty("terminalId", Required = Required.Always)]
    public string TerminalId { get; }

    public InitiateSessionRequest(string terminalId)
    {
        if (string.IsNullOrWhiteSpace(terminalId))
            throw new ArgumentException("TerminalId cannot be null or empty", nameof(terminalId));

        TerminalId = terminalId;
    }
}