using BaconographyPortable.Common;
using BaconographyPortable.Messages;
using BaconographyPortable.Model.Reddit;
using BaconographyPortable.ViewModel;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace BaconographyW8.View
{
    public sealed partial class NavigationView : UserControl
    {
		public NavigationView()
        {
            this.InitializeComponent();
        }

		private void Button_Tapped(object sender, TappedRoutedEventArgs e)
		{
			var button = sender as Button;
			var subreddit = button.DataContext as AboutSubredditViewModel;
			if (subreddit != null)
			{
				Messenger.Default.Send<SelectSubredditMessage>(new SelectSubredditMessage { Subreddit = subreddit.Thing, DontRefresh = false });
			}
		}
    }
}
