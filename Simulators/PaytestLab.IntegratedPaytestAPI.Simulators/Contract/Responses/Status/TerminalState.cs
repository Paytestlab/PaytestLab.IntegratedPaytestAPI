using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PaytestLab.IntegratedPaytestAPI.Contract.Responses.Status;

public enum TerminalState
{
    Unknown = 0,
    WaitingForCard = 1,
    ApplicationSelection = 2,
    PinEntry = 3,
    Authorizing = 4,
    TipEntry = 5,
    Dcc = 6,
    Installments = 7
}