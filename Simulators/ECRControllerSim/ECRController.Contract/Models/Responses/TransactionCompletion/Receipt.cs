using Newtonsoft.Json;
using System.Text.Json;

namespace ECRController.Contract.Models.Responses.TransactionCompletion;

public class Receipt
{
    [JsonProperty("content")]
    public string Content { get; set; }
}