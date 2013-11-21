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
        bool IsAPI(string originalUrl)
        {
            return YouTube.IsAPI(originalUrl);
        }

        Task<VideoResult> GetPlayableStreams(string originalUrl)
        {
            return YouTube.GetPlayableStreams(originalUrl);
        }
    }
}
