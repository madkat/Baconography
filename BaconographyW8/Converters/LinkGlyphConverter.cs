using BaconographyPortable.Services;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI;
using BaconographyPortable.Common;

namespace BaconographyW8.Converters
{
	/*
	 * Converter that takes a LinkViewModel to determine the type of glyph that should be displayed.
	 * The glyphs are from Segoe UI Symbol which is documented on MSDN: http://msdn.microsoft.com/en-us/library/windows/apps/jj841126.aspx
	 * If an appropriate glyph cannot be determined, a web glyph will be returned
	 */
	public class LinkGlyphConverter : IValueConverter
    {
		public object Convert(object value, Type targetType, object parameter, string language)
		{
            return LinkGlyphUtility.GetLinkGlyph(value);
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
    }
}
