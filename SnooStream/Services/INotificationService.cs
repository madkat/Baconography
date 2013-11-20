using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnooStream.Services
{
    public interface INotificationService
    {
        Task Report(string message, Action operation);
        Task Report(string message, Func<Task> operation);
        Task ReportWithProgress(string message, Func<Action<int>, Task> operation);
    }
}
