using CommonImageAquisition;
using CommonVideoAquisition;
using GalaSoft.MvvmLight;
using SnooSharp;
using SnooStream.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnooStream.ViewModel
{
    public class LinkViewModel : ViewModelBase
    {
        public LinkViewModel(ViewModelBase context, Link link)
        {
            Context = context;
            Link = link;
            _content = new Lazy<Task<ContentViewModel>>(LoadContent);

        }

        private async Task<ContentViewModel> LoadContent()
        {
            try
            {
                string targetHost = null;
                string fileName = null;

                if(Uri.IsWellFormedUriString(Link.Url, UriKind.RelativeOrAbsolute))
                {
                    var uri = new Uri(Link.Url);
                    targetHost = uri.DnsSafeHost.ToLower();
                    fileName = uri.AbsolutePath;
                }


                if(Link.IsSelf)
                    return new SelfContentViewModel(this, Link);
                else if(LinkGlyphUtility.IsComment(Link.Url) ||
                    LinkGlyphUtility.IsCommentsPage(Link.Url) ||
                    LinkGlyphUtility.IsSubreddit(Link.Url) ||
                    LinkGlyphUtility.IsUser(Link.Url) ||
                    LinkGlyphUtility.IsUserMultiReddit(Link.Url))
                {
                    return new InternalRedditContentViewModel(this, Link.Url);
                }
                else if (targetHost == "www.youtube.com" ||
                    targetHost == "www.youtu.be" ||
                    targetHost == "youtu.be" ||
                    targetHost == "youtube.com" ||
                    targetHost == "vimeo.com" ||
                    targetHost == "www.vimeo.com" ||
                    targetHost == "liveleak.com" ||
                    targetHost == "www.liveleak.com")
                {
                    if (VideoAquisition.IsAPI(Link.Url))
                    {
                        return new VideoViewModel(this, Link.Url);
                    }
                    else
                    {
                        return new WebViewModel(this, true, Link.Url);
                    }
                }
                else if (targetHost == "www.imgur.com" ||
                    targetHost == "imgur.com" ||
                    targetHost == "i.imgur.com" ||
                    targetHost == "min.us" ||
                    targetHost == "www.quickmeme.com" ||
                    targetHost == "www.livememe.com" ||
                    targetHost == "livememe.com" ||
                    targetHost == "i.qkme.me" ||
                    targetHost == "quickmeme.com" ||
                    targetHost == "qkme.me" ||
                    targetHost == "memecrunch.com" ||
                    targetHost == "flickr.com" ||
                    targetHost == "www.flickr.com" ||
                    fileName.EndsWith(".jpg") ||
                    fileName.EndsWith(".gif") ||
                    fileName.EndsWith(".png") ||
                    fileName.EndsWith(".jpeg"))
                {
                    if (ImageAquisition.IsImageAPI(Link.Url))
                    {
                        var imageApiResults = await ImageAquisition.GetImagesFromUrl(Link.Title, Link.Url);
                        if (imageApiResults != null && imageApiResults.Count() > 1)
                            return new AlbumViewModel(this, Link.Url, imageApiResults, Link.Title);
                        else if (imageApiResults != null)
                            return new ImageViewModel(this, imageApiResults.First().Item2, imageApiResults.First().Item1, null);
                    }
                    return new WebViewModel(this, true, Link.Url);
                }
                else
                    return new WebViewModel(this, false, Link.Url);
            }
            catch (Exception ex)
            {
                return new ErrorContentViewModel(this, ex);
            }
        }

        public ViewModelBase Context { get; private set; }
        private Lazy<Task<ContentViewModel>> _content;
        public ContentViewModel Content
        {
            get
            {
                if (_content.Value.IsCompleted)
                    return _content.Value.Result;
                else
                    return null;
            }
        }
        public CommentsViewModel Comments { get; private set; }
        public Link Link { get; private set; }
        public int CommentsLastViewed { get; private set; }
        //need to add load helpers here for kicking off preview loads when we get near things

        internal void MergeLink(Link link)
        {
            throw new NotImplementedException();
        }
    }
}
