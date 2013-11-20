using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnooStream.Common
{
    public class InitializationBlob
    {
        public Dictionary<string, string> Settings { get; set; }
        public Dictionary<string, bool> NSFWFilter { get; set; }

    }
}
