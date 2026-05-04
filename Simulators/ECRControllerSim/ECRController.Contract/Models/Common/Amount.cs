using Newtonsoft.Json;

namespace ECRController.Contract.Models.Common;

public class Amount
{
    [JsonProperty("value")]
    public double Value { get; set; }

    [JsonProperty("currency")]
    public string Currency { get; set; }
}