using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnuStream.ViewModel
{
    public class ImageViewModel : ContentViewModel
    {
        public string Url { get; set; }
        public string Domain { get; set; }
        public string Title { get; set; }
        public byte[] ImageSource { get; set; }
        public byte[] Preview { get; set; }
        public bool IsGif
        {
            get
            {
                return
                    ImageSource[0] == 0x47 && // G
                    ImageSource[1] == 0x49 && // I
                    ImageSource[2] == 0x46 && // F
                    ImageSource[3] == 0x38 && // 8
                    (ImageSource[4] == 0x39 || ImageSource[4] == 0x37) && // 9 or 7
                    ImageSource[5] == 0x61;   // a
            }
             
        }

        private async Task<byte[]> LoadImage()
        {
            await SnooStreamViewModel.NotificationService.ReportWithProgress("loading from " + Domain,
                async (report) =>
                {
                    var bytes = await SnooStreamViewModel.SystemServices.DownloadWithProgress(Url, report, SnooStreamViewModel.UIContextCancellationToken);
                    if (bytes != null && bytes.Length > 6) //minimum to identify the image type
                    {
                        
                    }
                });
        }

        public override async void LoadContent()
        {
            
        }

        public override void LoadPreview()
        {
            throw new NotImplementedException();
        }
    }
}
