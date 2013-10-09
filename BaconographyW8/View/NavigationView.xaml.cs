using BaconographyPortable.Common;
using BaconographyPortable.Messages;
using BaconographyPortable.Model.Reddit;
using BaconographyPortable.ViewModel;
using BaconographyW8.Common;
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

		private void Sidebar_Tapped(object sender, TappedRoutedEventArgs e)
		{
			if (sidebar.Visibility == Windows.UI.Xaml.Visibility.Collapsed)
			{
				sidebar.Visibility = Windows.UI.Xaml.Visibility.Visible;
			}
			else
			{
				sidebar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
			}
		}

		private void Subreddits_Tapped(object sender, TappedRoutedEventArgs e)
		{
			if (subredditList.Visibility == Windows.UI.Xaml.Visibility.Collapsed)
			{
				subredditList.Visibility = Windows.UI.Xaml.Visibility.Visible;
			}
			else
			{
				subredditList.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
			}
		}

		Flyout _redditPickerFlyout;
		private void ShowRedditPicker(object sender, RoutedEventArgs e)
		{
			App.SetSearchKeyboard(false);
			_redditPickerFlyout = new Flyout();
			_redditPickerFlyout.Width = 430;
			_redditPickerFlyout.Closed += (obj1, obj2) => App.SetSearchKeyboard(true);
			_redditPickerFlyout.Placement = PlacementMode.Bottom;
			_redditPickerFlyout.PlacementTarget = sender as UIElement;
			_redditPickerFlyout.Content = new SubredditPickerView();
			_redditPickerFlyout.IsOpen = true;
		}
    }
}
