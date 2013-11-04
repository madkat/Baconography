using BaconographyPortable.Model.Reddit;
using BaconographyPortable.Services;
using BaconographyPortable.ViewModel;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaconographyPortable.Common
{
    public static class OfflineUtility
    {
        public static async Task FullyOfflineLink(TypedThing<Link> linkThing)
        {
            try
            {
                var offlineService = ServiceLocator.Current.GetInstance<IOfflineService>();
                var redditService = ServiceLocator.Current.GetInstance<IRedditService>();
                var httpService = ServiceLocator.Current.GetInstance<ISimpleHttpService>();

                var glyph = LinkGlyphUtility.GetLinkGlyph(linkThing.Data);
                switch (glyph)
                {
                    case LinkGlyphUtility.PhotoGlyph:
                        await httpService.SendGet(null, linkThing.Data.Url); //get it into the cache
                        break;
                    case LinkGlyphUtility.WebGlyph:
                        var loadedReadableArticle = await ReadableArticleViewModel.LoadFully(httpService, linkThing.Data.Url, linkThing.Data.Id);
                        await offlineService.StoreBlob("boilerpipe:" + linkThing.Data.Url, new Tuple<object[], string, string>(loadedReadableArticle.ArticleParts.ToArray(), loadedReadableArticle.Title, loadedReadableArticle.LinkId));
                        break;
                    case LinkGlyphUtility.CommentGlyph:
                        var targetLinkPermalink = linkThing.Data.Url.Substring(linkThing.Data.Url.IndexOf("reddit.com") + "reddit.com".Length);
                        var targetSubreddit = targetLinkPermalink.Substring("/r/".Length, targetLinkPermalink.IndexOf("/", "/r/".Length) - "/r/".Length);
                        var linkedCommentsListing = await redditService.GetCommentsOnPost(targetSubreddit, targetLinkPermalink, null);
                        await offlineService.StoreComments(linkedCommentsListing);
                        break;
                }

                var commentsListing = await redditService.GetCommentsOnPost(linkThing.Data.Subreddit, linkThing.Data.Permalink, null);
                await offlineService.StoreComments(commentsListing);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        public static async Task<ReadableArticleViewModel> GetReadableArticle(string url)
        {
            try
            {
                var offlineService = ServiceLocator.Current.GetInstance<IOfflineService>();
                var blobTpl = await offlineService.RetriveBlob<Tuple<object[], string, string>>("boilerpipe:" + url, TimeSpan.FromDays(1024));
                if (blobTpl != null)
                    return new ReadableArticleViewModel { ArticleUrl = url, LinkId = blobTpl.Item3, ArticleParts = new System.Collections.ObjectModel.ObservableCollection<object>(blobTpl.Item1), Title = blobTpl.Item2 };
                else
                    return null;
            }
            catch 
            {
                return null;
            }
        }
    }
}
