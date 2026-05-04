namespace ECRController.Core.Utils;

public static class ApiPaths
{
    public const string TransactionStart = "/transaction/start";
    public const string TransactionAbort = "/transaction/abort";
    public const string Status = "/status";
    public const string WebhooksRegister = "/webhooks/register";
    public const string WebhooksUnregister = "/webhooks/unregister";

    public static string StatusWithTerminal(string terminalId) => $"{Status}?terminalId={Uri.EscapeDataString(terminalId)}";
}