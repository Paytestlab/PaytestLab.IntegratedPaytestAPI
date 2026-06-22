using Newtonsoft.Json;

namespace PaytestLab.IntegratedPaytestAPI.Contract.Common;

public class Amount
{
    [JsonProperty("value")]
    public decimal Value { get; }

    [JsonProperty("currency")]
    public string Currency { get; }

    public Amount(string currency, decimal value = 0m)
    {
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency cannot be null or empty.", nameof(currency));

        if (value < 0)
            throw new ArgumentOutOfRangeException(nameof(value), "Value cannot be negative.");

        Value = value;
        Currency = currency;
    }
}