﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnuStream.Services
{
    interface ISystemServices
    {
        object StartTimer(EventHandler<object> tickHandler, TimeSpan tickSpan, bool uiThread);
        void RestartTimer(object tickHandle);
        void StopTimer(object tickHandle);
        void RunAsync(Func<object, Task> action);
        void StartThreadPoolTimer(Func<object, Task> action, TimeSpan timer);
        bool IsOnMeteredConnection { get; }
        bool IsNearingDataLimit { get; }
    }
}
