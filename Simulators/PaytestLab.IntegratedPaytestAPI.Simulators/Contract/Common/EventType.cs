using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace PaytestLab.IntegratedPaytestAPI.Contract.Common;

[JsonConverter(typeof(StringEnumConverter))]
public enum EventType
{
    [EnumMember(Value = "terminalStatus")]
    Status
}