using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace PaytestLab.IntegratedPaytestAPI.Contract.Common;

[JsonConverter(typeof(StringEnumConverter))]
public enum TransactionType
{
    [EnumMember(Value = "Sale")]
    Sale = 0,

    [EnumMember(Value = "Purchase")]
    Purchase = 1,

    [EnumMember(Value = "Cashback")]
    Cashback = 2,

    [EnumMember(Value = "Refund")]
    Refund = 3,

    [EnumMember(Value = "PreAuthorization")]
    PreAuthorization = 4,

    [EnumMember(Value = "OnlineAdvice")]
    OnlineAdvice = 5,

    [EnumMember(Value = "PurchaseWithCashback")]
    PurchaseWithCashback = 6
}