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
        public AlbumViewModel(string originalUrl, IEnumerable<Tuple<string, string>> apiResults, string albumTitle)
        {
            Url = originalUrl;
            Domain = new Uri(originalUrl).Host;
            AlbumTitle = albumTitle;
            Images = new ObservableCollection<ImageViewModel>();
            ApiResults = apiResults.Where(tpl => Uri.IsWellFormedUriString(tpl.Item1, UriKind.Absolute)).ToList();
            ApiImageCount = ApiResults.Count();
            if (ApiImageCount == 0)
                throw new Exception(string.Format("Invalid Album {0}", originalUrl));
        }

        private async void LoadAlbumImpl()
        {
            int i = 0;
            foreach (var tpl in ApiResults)
            {
                if(Uri.IsWellFormedUriString(tpl.Item1, UriKind.RelativeOrAbsolute))
                {
                    var imageUri = new Uri(tpl.Item1);
                    //make sure we havent already loaded this image
                    if (Images.Count <= i)
                    {
                        if (await LoadImageImpl(tpl.Item2, imageUri))
                            i++;
                    }
                }
                
            }
        }
        private async Task<bool> LoadImageImpl(string title, Uri source)
        {
            bool loadedOne = false;
            await SnooStreamViewModel.NotificationService.ReportWithProgress("loading from " + source.Host,
                async (report) =>
                {
                    var bytes = await SnooStreamViewModel.SystemServices.DownloadWithProgress(source.ToString(), report, SnooStreamViewModel.UIContextCancellationToken);
                    if (bytes != null && bytes.Length > 6) //minimum to identify the image type
                    {
                        loadedOne = true;
                        Images.Add(new ImageViewModel { ImageSource = bytes, Title = title, Url = source.ToString(), Domain = source.Host });
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

        public override void LoadContent()
        {
            LoadAlbumImpl();
        }

        public override void LoadPreview()
        {
            var firstImage = ApiResults.First();
            LoadImageImpl(firstImage.Item2, new Uri(firstImage.Item1));
        }
    }
}
