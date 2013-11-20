using SnuStream.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnuStreamWP8.PlatformServices
{
    class MarkdownProvider : IMarkdownProcessor
    {
        public MarkdownData Process(string markdown)
        {
            var processed = SnuDomWP8.SnuDom.MarkdownToDOM(System.Net.WebUtility.HtmlDecode(markdown));
            return new MarkdownData { MarkdownDom = processed };
        }
    }
}
