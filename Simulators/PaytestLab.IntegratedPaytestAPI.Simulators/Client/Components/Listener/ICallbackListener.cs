using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaytestLab.IntegratedPaytestAPI.Client.Components.Listener;

public interface ICallbackListener : IDisposable
{
    event Func<string, string, Task> CallbackReceived;
    event Func<string, Task> LogCallbackReceived;
}