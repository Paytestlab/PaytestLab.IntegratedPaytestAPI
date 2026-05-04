using Newtonsoft.Json;

namespace ECRController.Contract.Models.Requests.StartTransaction;

public class TestCaseInformation
{
    [JsonProperty("testRunId")]
    public string TestRunId { get; set; }

    [JsonProperty("testCaseId")]
    public string TestCaseId { get; set; }
}