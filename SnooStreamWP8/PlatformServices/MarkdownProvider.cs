using SnooStream.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnooStreamWP8.PlatformServices
{
    class MarkdownProvider : IMarkdownProcessor
    {
        public MarkdownData Process(string markdown)
        {
            var processed = SnooDomWP8.SnooDom.MarkdownToDOM(System.Net.WebUtility.HtmlDecode(markdown));
            return new MarkdownData { MarkdownDom = processed };
        }
    }
}
