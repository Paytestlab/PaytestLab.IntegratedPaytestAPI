namespace ECRController.Core.Listener;

public interface ICallbackListener : IDisposable, IAsyncDisposable
{
    event Func<string, string, Task> CallbackReceived;
    event Func<string, Task> LogCallbackReceived;
}