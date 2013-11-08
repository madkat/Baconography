using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnuSharp
{
    public interface IActionDeferralSink
    {
        event Action HasDeferrals;
        void Defer(Dictionary<string, string> arguments, string action);
        Task<Tuple<Dictionary<string, string>, string>> DequeDeferral();
    }
}
