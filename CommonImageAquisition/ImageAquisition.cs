using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonImageAquisition
{
    public class ImageAquisition
    {

        public static async Task<IEnumerable<Tuple<string, string>>> GetImagesFromUrl(string title, string url)
        {
            try
            {
                var uri = new Uri(url);

                string filename = Path.GetFileName(uri.LocalPath);

                if (filename.EndsWith(".jpg") || filename.EndsWith(".png") || filename.EndsWith(".jpeg") || filename.EndsWith(".gif"))
                    return new Tuple<string, string>[] { Tuple.Create(title, url) };
                else
                {
                    var targetHost = uri.DnsSafeHost.ToLower(); //make sure we can compare caseless

                    switch (targetHost)
                    {
                        case "imgur.com":
                        case "i.imgur.com":
                            return await Imgur.GetImagesFromUri(title, uri);
                        case "min.us":
                            return await Minus.GetImagesFromUri(title, uri);
                        case "www.quickmeme.com":
                        case "i.qkme.me":
                        case "quickmeme.com":
                        case "qkme.me":
                            return Quickmeme.GetImagesFromUri(title, uri);
                        case "memecrunch.com":
                            return Memecrunch.GetImagesFromUri(title, uri);
                        case "flickr.com":
                        case "www.flickr.com":
                            return await Flickr.GetImagesFromUri(title, uri);
                        default:
                            return Enumerable.Empty<Tuple<string, string>>();
                    }
                }
            }
            catch
            {
                return Enumerable.Empty<Tuple<string, string>>();
            }
        }

        public static bool MightHaveImagesFromUrl(string url)
        {
            try
            {
                var uri = new Uri(url);

                string filename = Path.GetFileName(uri.LocalPath);

                if (filename.EndsWith(".jpg") || url.EndsWith(".png") || url.EndsWith(".jpeg") || filename.EndsWith(".gif"))
                    return true;
                else
                {
                    var targetHost = uri.DnsSafeHost.ToLower(); //make sure we can compare caseless

                    switch (targetHost)
                    {
                        case "imgur.com":
                        case "i.imgur.com":
                        case "min.us":
                        case "www.quickmeme.com":
                        case "i.qkme.me":
                        case "quickmeme.com":
                        case "qkme.me":
                        case "memecrunch.com":
                        case "www.flickr.com":
                        case "flickr.com":
                            return true;
                    }
                }

            }
            catch
            {
                //ignore failure here, we're going to return false anyway
            }
            return false;
        }


        public static bool IsImage(string url)
        {
            try
            {
                var uri = new Uri(url);

                string filename = Path.GetFileName(uri.LocalPath);

                if (filename.EndsWith(".jpg") || url.EndsWith(".png") || url.EndsWith(".jpeg") || filename.EndsWith(".gif"))
                    return true;
            }
            catch
            {
                //ignore failure here, we're going to return false anyway
            }
            return false;
        }

        public static bool IsImageAPI(string url)
        {
            try
            {
                var uri = new Uri(url);

                string filename = Path.GetFileName(uri.LocalPath);

                if (filename.EndsWith(".jpg") || url.EndsWith(".png") || url.EndsWith(".jpeg") || filename.EndsWith(".gif"))
                    return false;
                else
                {
                    var targetHost = uri.DnsSafeHost.ToLower(); //make sure we can compare caseless

                    switch (targetHost)
                    {
                        case "imgur.com":
                        case "i.imgur.com":
                            return Imgur.IsAPI(uri);
                        case "min.us":
                            return Minus.IsAPI(uri);
                        case "flickr.com":
                        case "www.flickr.com":
                            return Flickr.IsAPI(uri);
                        case "www.quickmeme.com":
                        case "i.qkme.me":
                        case "quickmeme.com":
                        case "qkme.me":
                        case "memecrunch.com":
                            return false;
                    }
                }

            }
            catch
            {
                //ignore failure here, we're going to return false anyway
            }
            return false;
        }
    }
}
