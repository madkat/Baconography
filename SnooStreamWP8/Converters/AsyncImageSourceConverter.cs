using SnooStream.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace SnooStreamWP8.Converters
{
    public class AsyncImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var result = new BitmapImage();
            var imageSource = value as SnooStream.Common.ImageSource;
            imageSource.ImageData.ContinueWith(tsk =>
                {
                    if (tsk.IsCompleted)
                        result.SetSource(new MemoryStream(tsk.Result));
                    else if(Uri.IsWellFormedUriString(imageSource.UrlSource, UriKind.Absolute))
                        result.UriSource = new Uri(imageSource.UrlSource);
                }, SnooStreamViewModel.UIScheduler);

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
