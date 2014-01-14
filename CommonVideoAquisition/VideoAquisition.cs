using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonVideoAquisition
{
    public class VideoAquisition
    {
        
        //someday we will support other video providers
        public static bool IsAPI(string originalUrl)
        {
            return YouTube.IsAPI(originalUrl);
        }

        public static Task<VideoResult> GetPlayableStreams(string originalUrl, Func<string, Task<string>> getter)
        {
            return YouTube.GetPlayableStreams(originalUrl, getter);
        }
    }
}
