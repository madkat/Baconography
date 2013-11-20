using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CommonVideoAquisition
{
    class YouTube
    {
        public static bool IsAPI(string url)
        {
            var youtubeRegex = new Regex("youtu(?:\\.be|be\\.com)/(?:.*v(?:/|=)|(?:.*/)?)([a-zA-Z0-9-_]+)");
            return youtubeRegex.IsMatch(url);
        }
        
        private static string CleanName(string dirtyName)
        {
            switch (dirtyName)
            {
                case "video/webm;+codecs":
                    return "webm";
                case "video/mp4;+codecs":
                    return "mp4";
                case "video/flv":
                    return "flash";
                case "video/3gpp":
                    return "mobile";
                default:
                    return "unknown";
            }
        }

        static string MakeUsableUrl(Dictionary<string, string> raw)
        {
            var rawUrl = Uri.UnescapeDataString(raw["url"]);
            var rawSig = raw["sig"];
            return rawUrl + "&signature=" + rawSig;
        }

        static string MakeQualityString(Dictionary<string, string> raw)
        {
            int itag = 0;
            if (int.TryParse(raw["itag"], out itag))
            {
                switch (itag)
                {
                    case 5:
                    case 36:
                    case 83:
                    case 133:
                        return "240p";
                    case 34:
                    case 43:
                    case 82:
                    case 100:
                    case 101:
                    case 134:
                        return "360p";
                    case 35:
                    case 44:
                    case 135:
                        return "480p";
                    case 22:
                    case 45:
                    case 84:
                    case 102:
                    case 120:
                    case 136:
                        return "720p";
                    case 37:
                    case 46:
                    case 137:
                        return "1080p";

                }
            }
            return "unknown";
        }

        public static async Task<IEnumerable<Tuple<string, string>>> GetPlayableStreams(string originalUrl)
        {
            var streams = await GetPlayableStreamsImpl(originalUrl);
            return streams.Select(dict => Tuple.Create(MakeUsableUrl(dict), MakeQualityString(dict)));
        }

        private static async Task<IEnumerable<Dictionary<string, string>>> GetPlayableStreamsImpl(string originalUrl)
        {
            //check which video provider we are
            var youtubeRegex = new Regex("youtu(?:\\.be|be\\.com)/(?:.*v(?:/|=)|(?:.*/)?)([a-zA-Z0-9-_]+)");
            if (youtubeRegex.IsMatch(originalUrl))
            {
                //need to sanitize the url since we're trying to get the html5 version if at all possible
                string youtubeId = youtubeRegex.Match(originalUrl).Groups[1].Value;
                var html5YoutubeUrl = string.Format("http://www.youtube.com/watch?v={0}&nomobile=1", youtubeId);
                using(var clientHandler = new HttpClientHandler())
                {
                    using (var client = new HttpClient())
                    {
                        var html5PageContents = await client.GetStringAsync(html5YoutubeUrl);
                        return GetUrls(html5PageContents);
                    }
                }
                
            }
            else
                return null;
        }

        // "url_encoded_fmt_stream_map": "
        private static Regex _streamMapRegex = new Regex("\"url_encoded_fmt_stream_map\":\\s*\"([^\"]+)\"");
        private static IEnumerable<Dictionary<string, string>> GetUrls(string pageContents)
        {
            var streamMapMatch = _streamMapRegex.Match(pageContents);
            if (streamMapMatch.Groups != null &&
                streamMapMatch.Groups.Count > 1 &&
                streamMapMatch.Groups[1].Value != null)
            {
                var temp1 = streamMapMatch.Groups[1].Value.Split(',')
                    .Select(str => Uri.UnescapeDataString(str))
                    .ToList();

                var parsedStreamMap = temp1
                    .Select(str => MakeUniqueDictionary(SplitAmp(str)))
                    .Where(elem => elem.ContainsKey("itag") && elem.ContainsKey("type") && elem.ContainsKey("sig") && elem.ContainsKey("url"))
                    //need to take video stream type into account for preference, mp4 is the most playable stream available here
                    .OrderByDescending(elem => scoreFileType(elem["type"]))
                    .ToList();

                if (parsedStreamMap.Count > 0)
                    return parsedStreamMap;
                else
                    return null;
            }
            return null;
        }

        static int scoreFileType(string type)
        {
            if (type.StartsWith("video/mp4"))
                return 100;
            else if (type.StartsWith("video/3gp"))
                return 50;
            else
                return 1;
        }

        static string ampFubar = (char)92 + "u0026";

        private static IEnumerable<Tuple<string, string>> SplitAmp(string str)
        {
            List<Tuple<string, string>> results = new List<Tuple<string, string>>();
            foreach (var strSub in str.Split(new string[] { ampFubar }, StringSplitOptions.RemoveEmptyEntries))
            {
                var strSubIndex = strSub.IndexOf('=');
                if (strSubIndex != -1)
                    results.Add(Tuple.Create(strSub.Remove(strSubIndex), strSub.Substring(strSubIndex + 1)));
            }
            return results;
        }

        private static Dictionary<string, string> MakeUniqueDictionary(IEnumerable<Tuple<string, string>> elements)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            foreach (var element in elements)
            {
                if (!result.ContainsKey(element.Item1))
                    result.Add(element.Item1, element.Item2);
            }
            return result;
        }
    }
}
