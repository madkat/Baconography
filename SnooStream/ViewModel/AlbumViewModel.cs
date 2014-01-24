using GalaSoft.MvvmLight;
using SnooStream.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnooStream.ViewModel
{
    public class AlbumViewModel : ContentViewModel
    {
        public AlbumViewModel(ViewModelBase context, string originalUrl, IEnumerable<Tuple<string, string>> apiResults, string albumTitle) : base(context)
        {
            Url = originalUrl;
            Domain = new Uri(originalUrl).Host;
            AlbumTitle = albumTitle;
            Images = new ObservableCollection<ImageViewModel>();
            ApiResults = apiResults.Where(tpl => Uri.IsWellFormedUriString(tpl.Item2, UriKind.Absolute)).ToList();
            ApiImageCount = ApiResults.Count();
            if (ApiImageCount == 0)
                throw new Exception(string.Format("Invalid Album {0}", originalUrl));
        }

        private async void LoadAlbumImpl()
        {
            int i = 0;
            foreach (var tpl in ApiResults)
            {
                if(Uri.IsWellFormedUriString(tpl.Item2, UriKind.RelativeOrAbsolute))
                {
                    var imageUri = new Uri(tpl.Item2);
                    //make sure we havent already loaded this image
                    if (Images.Count <= i)
                    {
                        if (await LoadImageImpl(tpl.Item1, imageUri, false))
                            i++;
                    }
                }
                
            }
        }
        private async Task<bool> LoadImageImpl(string title, Uri source, bool isPreview)
        {
            bool loadedOne = false;
            await SnooStreamViewModel.NotificationService.ReportWithProgress("loading from " + source.Host,
                async (report) =>
                {
                    var bytes = await SnooStreamViewModel.SystemServices.DownloadWithProgress(source.ToString(),
                        isPreview ? (progress) => report(PreviewLoadPercent = progress) : report, 
                        SnooStreamViewModel.UIContextCancellationToken);
                    if (bytes != null && bytes.Length > 6) //minimum to identify the image type
                    {
                        loadedOne = true;
                        Images.Add(new ImageViewModel(this, source.ToString(), title, new ImageSource(source.ToString(), bytes)));
                    }
                });
            return loadedOne;
        }

        private IEnumerable<Tuple<string, string>> ApiResults { get; set; }
        public string Url { get; private set; }
        public string Domain { get; private set; }
        public int ApiImageCount { get; private set; }
        public ObservableCollection<ImageViewModel> Images { get; private set; }
        public string AlbumTitle { get; private set; }
        public PreviewImageSource Preview { get; private set; }

        protected override async Task LoadContent()
        {
            var firstImage = ApiResults.First();
            var addResult = await LoadImageImpl(firstImage.Item1, new Uri(firstImage.Item2), true);
            if (addResult)
            {
                Preview = Images.First().Preview;
            }
            LoadAlbumImpl();
        }
    }
}
