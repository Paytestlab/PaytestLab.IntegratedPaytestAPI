using Newtonsoft.Json;

namespace PaytestLab.IntegratedPaytestAPI.Contract.Responses.TransactionCompletion;

public class Receipt
{
    [JsonProperty(nameof(Content))]
    public string Content { get; set; }
}