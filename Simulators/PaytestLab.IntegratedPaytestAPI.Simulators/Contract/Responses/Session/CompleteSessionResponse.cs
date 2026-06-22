using Newtonsoft.Json;

namespace PaytestLab.IntegratedPaytestAPI.Contract.Responses.Session;

public class CompleteSessionResponse
{
    [JsonProperty(nameof(SessionId), Required = Required.Always)]
    public string SessionId { get; set; }

    [JsonProperty(nameof(Result), Required = Required.Always)]
    public SessionResult Result { get; set; }

    [JsonProperty(nameof(TransactionCount), Required = Required.Always)]
    public int TransactionCount { get; set; }

    [JsonProperty(nameof(Transactions), Required = Required.Always)]
    public List<SessionTransactionSummary> Transactions { get; set; }

    [JsonProperty(nameof(ErrorMessage), NullValueHandling = NullValueHandling.Ignore)]
    public string ErrorMessage { get; set; }
}