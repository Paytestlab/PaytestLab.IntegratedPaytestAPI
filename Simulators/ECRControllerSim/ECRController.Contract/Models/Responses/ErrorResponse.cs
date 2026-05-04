using Newtonsoft.Json;

namespace ECRController.Contract.Models.Responses;

public class ErrorResponse
{
    [JsonProperty("message", Required = Required.Always)]
    public string Message { get; set; }
}