using SnooSharp;
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

        public string AfterSelfMessage { get; set; }
        public string AfterSelfSentMessage { get; set; }
        public string AfterSelfAction { get; set; }
        public IEnumerable<Thing> SelfThings { get; set; }
    }
}
