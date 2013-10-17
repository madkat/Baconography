using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace BaconographyW8.Converters
{
    public class SortOrderIndexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
			if (value is string)
			{
				string val = value as string;
				switch (val)
				{
					
					case "/new/":
						return 1;
					case "/rising/":
						return 2;
					case "/controversial/":
						return 3;
					case "/top/":
						return 4;
					case "/gilded/":
						return 5;
					case "":
					default:
						return 0;
				}
			}
			else
				return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
			if (value is Nullable<int>)
			{
				var val = value as Nullable<int>;
				if (val.HasValue)
				{
					switch (val.Value)
					{
						case 1:
							return "new";
						case 2:
							return "rising";
						case 3:
							return "controversial";
						case 4:
							return "top";
						case 5:
							return "gilded";
						case 0:
						default:
							return "hot";
					}
				}
			}
			return "";
        }
    }
}
