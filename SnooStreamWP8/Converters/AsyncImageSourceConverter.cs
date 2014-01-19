using SnooStream.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using SnooStream.Common;

namespace SnooStreamWP8.Converters
{
    public class AsyncImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var result = new BitmapImage();
            var imageSource = value as SnooStream.Common.ImageSource;
            if (imageSource != null)
            {
                imageSource.ImageData.ContinueWith(tsk =>
                    {
                        var tskResult = tsk.TryValue();
                        if (tskResult != null)
                            result.SetSource(new MemoryStream(tskResult));
                        else if (Uri.IsWellFormedUriString(imageSource.UrlSource, UriKind.Absolute))
                            result.UriSource = new Uri(imageSource.UrlSource);
                    }, SnooStreamViewModel.UIScheduler);
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
