using Newtonsoft.Json;

namespace PaytestLab.IntegratedPaytestAPI.Client.Components.Utils;

public static class JsonHelper
{
    public static readonly JsonSerializerSettings Settings = new()
    {
        NullValueHandling = NullValueHandling.Ignore,
        MissingMemberHandling = MissingMemberHandling.Ignore,
        DateParseHandling = DateParseHandling.DateTimeOffset
    };
}
