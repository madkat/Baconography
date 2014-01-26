using GalaSoft.MvvmLight;
using NBoilerpipePortable.Extractors;
using NBoilerpipePortable.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net;

namespace SnooStream.ViewModel
{
    public class WebViewModel : ContentViewModel
    {
        public bool NoPreview { get; private set; }
        public bool TextPreview { get; private set; }
        public bool ImagePreview { get; private set; }
        public bool NotText { get; private set; }
        public string Url { get; private set; }
        public string Title { get; private set; }
        public ObservableCollection<object> WebParts { get; private set; }

        public WebViewModel(ViewModelBase context, bool notText, string url) : base(context)
        {
            TextPreview = !notText;
            Url = url;
            WebParts = new ObservableCollection<object>();
        }

        //preview could look for a large image in the content or it could grab the first paragraph
        //public override void LoadContent()
        //{
            //if nboilerpipe is turned off dont do anything here
            //otherwise load everything
        //}

        //public override void LoadPreview()
        //{
            //useing nboilerpipe grab the article and get either the first paragraph or the first big image near the top
        //}

        protected override async Task LoadContent()
        {
            using(var handler = new HttpClientHandler { CookieContainer = new System.Net.CookieContainer() })
            {
                if (handler.SupportsAutomaticDecompression)
                {
                    handler.AutomaticDecompression = DecompressionMethods.GZip |
                                                     DecompressionMethods.Deflate;
                }
                using (var client = new HttpClient(handler))
                {
                    var linkContext = Context as LinkViewModel;
                    await LoadFully(client, Url, linkContext != null ? linkContext.Link.Name : null);
                }
            }
            
            
            
        }

        private async Task LoadFully(HttpClient httpService, string url, string linkId)
        {
            var source = new Uri(url);

            await SnooStreamViewModel.NotificationService.ReportWithProgress("loading from " + source.Host,
                async (report) =>
                {
                    string nextUrl = url;

                    int i = 0;
                    //max out at 8 pages so we dont run forever on wierd page designs
                    while (!string.IsNullOrEmpty(nextUrl) && i++ < 8)
                    {
                        List<object> result = new List<object>();
                        var loadResult = await LoadOneImpl(httpService, nextUrl, result);

                        //need to do these things on the UI thread since we're trying to trigger a UI response
                        await Task.Factory.StartNew(() =>
                        {
                            if (Title == null)
                            {
                                Title = loadResult.Item2;
                                RaisePropertyChanged("Title");
                            }
                            bool hasImage = false;
                            bool hasText = false;

                            foreach (var item in result)
                            {
                                if (item is ReadableArticleImage)
                                    hasImage = true;
                                else if (item is ReadableArticleParagraph)
                                    hasText = true;

                                WebParts.Add(item);
                            }

                            if (i == 0)
                            {
                                NoPreview = (!hasImage && !hasText);
                                TextPreview = (!hasImage && hasText);
                                ImagePreview = hasImage;
                                NotText = !hasText;

                                RaisePropertyChanged("NoPreview");
                                RaisePropertyChanged("TextPreview");
                                RaisePropertyChanged("ImagePreview");
                                RaisePropertyChanged("NotText");
                            }

                            report(i * 10);
                        }, SnooStreamViewModel.UIContextCancellationToken, TaskCreationOptions.None, SnooStreamViewModel.UIScheduler);

                        nextUrl = loadResult.Item1;
                    }
                });
        }

        private static async Task<Tuple<string, string>> LoadOneImpl(HttpClient httpClient, string url, IList<Object> target)
        {
            string domain = url;
            if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
                domain = new Uri(url).Authority;
            var page = await httpClient.GetStringAsync(url);
            string title;
            var pageBlocks = ArticleExtractor.INSTANCE.GetTextAndImageBlocks(page, new Uri(url), out title);
            foreach (var tpl in pageBlocks)
            {
                if (!string.IsNullOrEmpty(tpl.Item2))
                {
                    target.Add(new ReadableArticleImage { Url = tpl.Item2 });
                }

                StringBuilder articleContentsBuilder = new StringBuilder();
                foreach (var pp in tpl.Item1.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (target.Count > 200)
                        break;

                    articleContentsBuilder.AppendLine(pp);
                    if (articleContentsBuilder.Length > 1000)
                    {
                        target.Add(new ReadableArticleParagraph { Text = articleContentsBuilder.ToString() });
                        articleContentsBuilder.Clear();
                    }

                }
                if (articleContentsBuilder.Length > 0)
                {
                    target.Add(new ReadableArticleParagraph { Text = articleContentsBuilder.ToString() });
                }
            }
            var nextPageUrl = MultiPageUtils.FindNextPageLink(SgmlDomBuilder.GetBody(SgmlDomBuilder.BuildDocument(page)), url);
            return Tuple.Create(nextPageUrl, title);
        }
    }

    public class ReadableArticleParagraph
    {
        public string Text {get; set;}
    }

    public class ReadableArticleImage
    {
        public string Url {get; set;}
    }
}
