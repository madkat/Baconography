using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Baconography.PlatformServices.ImageAPI
{
    class Livememe
    {
        //Transliterated from Reddit Enhancement Suite https://github.com/honestbleeps/Reddit-Enhancement-Suite/blob/master/lib/reddit_enhancement_suite.user.js
        private static Regex hashRe = new Regex(@"^http:\/\/(?:www\.livememe\.com|lvme\.me)\/(?!edit)([\w]+)\/?");

        internal static IEnumerable<Tuple<string, string>> GetImagesFromUri(string title, Uri uri)
        {
            var href = uri.OriginalString;
            var groups = hashRe.Match(href).Groups;

            if (groups.Count > 0 && !string.IsNullOrWhiteSpace(groups[1].Value))
            {
                return new Tuple<string, string>[] { Tuple.Create(title, string.Format("http://www.livememe.com/{0}.jpg", groups[1].Value)) };
            }
            else
                return Enumerable.Empty<Tuple<string, string>>();
        }
    }
}
