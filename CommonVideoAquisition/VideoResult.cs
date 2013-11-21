using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonVideoAquisition
{
    public class VideoResult
    {
        public string PreviewUrl { get; set; }
        public IEnumerable<Tuple<string, string>> PlayableStreams { get; set; }
    }
}
