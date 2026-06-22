using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace PaytestLab.IntegratedPaytestAPI.Contract.Responses.TransactionCompletion;

[JsonConverter(typeof(StringEnumConverter))]
public enum TransactionStatus
{
    [EnumMember(Value = "Unknown")]
    Unknown = 0,
    [EnumMember(Value = "Success")]
    Success = 1,
    [EnumMember(Value = "Failed")]
    Failed = 2,
    [EnumMember(Value = "Approved")]
    Approved = 3,
    [EnumMember(Value = "Declined")]
    Declined = 4
}