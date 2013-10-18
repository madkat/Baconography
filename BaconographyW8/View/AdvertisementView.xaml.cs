using BaconographyW8.Common;
using Microsoft.Advertising.WinRT.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using Windows.UI.Xaml.Controls;

namespace BaconographyW8.View
{
    public partial class AdvertisementView : LayoutAwareUserControl
    {
        public AdvertisementView()
        {
            InitializeComponent();
        }

		private void advertisement_AdRefreshed(object sender, Windows.UI.Xaml.RoutedEventArgs e)
		{
			if (advertisement.Height == 0)
			{
				advertisement.Height = 90;
				advertisement.Visibility = Windows.UI.Xaml.Visibility.Visible;
				adDuplexAd.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
			}
		}

		private void advertisement_ErrorOccurred(object sender, AdErrorEventArgs e)
		{
			advertisement.Height = 0;
			advertisement.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
			adDuplexAd.Visibility = Windows.UI.Xaml.Visibility.Visible;
		}
    }
}
