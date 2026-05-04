using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace ECRController.Contract.Models.Requests.StartTransaction;

[JsonConverter(typeof(StringEnumConverter))]
public enum TransactionType
{
    [EnumMember(Value = "Sale")]
    Sale,

    [EnumMember(Value = "Purchase")]
    Purchase,

    [EnumMember(Value = "Cashback")]
    Cashback,

    [EnumMember(Value = "Refund")]
    Refund,

    [EnumMember(Value = "PreAuthorization")]
    PreAuthorization,

    [EnumMember(Value = "OnlineAdvice")]
    OnlineAdvice,

    [EnumMember(Value = "PurchaseWithCashback")]
    PurchaseWithCashback
}