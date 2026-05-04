using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace ECRController.Contract.Models.Requests.Webhook;

[JsonConverter(typeof(StringEnumConverter))]
public enum WebhookEvent
{
    [EnumMember(Value = "Status")]
    Status
}