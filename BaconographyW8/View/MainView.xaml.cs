using BaconographyPortable.Messages;
using BaconographyPortable.Model.Reddit;
using BaconographyPortable.ViewModel;
using BaconographyW8.Common;
using BaconographyW8.Messages;
using GalaSoft.MvvmLight.Messaging;
using Newtonsoft.Json;
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

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace BaconographyW8.View
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class MainView : BaconographyW8.Common.LayoutAwarePage
    {
        public MainView()
        {
            this.InitializeComponent();
			Messenger.Default.Register<CloseSubredditsMessage>(this, OnCloseSubreddits);
        }

		private async void OnCloseSubreddits(CloseSubredditsMessage message)
		{
			if (redditView.Visibility == Windows.UI.Xaml.Visibility.Collapsed)
			{
				redditView.Visibility = Windows.UI.Xaml.Visibility.Visible;
				subredditPicker.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
			}
		}

		string _sidebarState = "SidebarOpen";

		private void sidebarButton_Tapped(object sender, TappedRoutedEventArgs e)
		{
			if (navigationView.Width == 0)
			{
				_sidebarState = "SidebarOpen";
				
			}
			else
			{
				_sidebarState = "SidebarClosed";
			}

			VisualStateManager.GoToState(this, _sidebarState, false);
		}

		protected override string DetermineVisualState(Windows.UI.ViewManagement.ApplicationViewState viewState)
		{
			if (viewState == Windows.UI.ViewManagement.ApplicationViewState.FullScreenPortrait)
			{
				VisualStateManager.GoToState(this, "SidebarClosed", false);
			}
			else if (viewState == Windows.UI.ViewManagement.ApplicationViewState.FullScreenLandscape)
			{
				VisualStateManager.GoToState(this, _sidebarState, false);
			}
			return base.DetermineVisualState(viewState);
		}

		protected override void OnNavigatedFrom(NavigationEventArgs e)
		{
			if (_redditPickerFlyout != null)
				_redditPickerFlyout.IsOpen = false;

			base.OnNavigatedFrom(e);
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);
		}

		/// <summary>
		/// Populates the page with content passed during navigation.  Any saved state is also
		/// provided when recreating a page from a prior session.
		/// </summary>
		/// <param name="navigationParameter">The parameter value passed to
		/// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
		/// </param>
		/// <param name="pageState">A dictionary of state preserved by this page during an earlier
		/// session.  This will be null the first time a page is visited.</param>
		protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
		{
			if (pageState != null && pageState.ContainsKey("SidebarState"))
			{
				_sidebarState = pageState["SidebarState"] as string;

				VisualStateManager.GoToState(this, _sidebarState, false);
			}

			if (pageState != null && pageState.ContainsKey("ScrollOffset"))
			{
				redditView.ScrollOffset = pageState["ScrollOffset"] as Nullable<double> ?? 0.0;
				redditView.SetLinksLoadedEvent();
			}

			if (pageState != null && pageState.ContainsKey("SelectedSubredditMessage"))
			{
				redditView.SubredditInfo = pageState["SelectedSubredditMessage"] as SelectSubredditMessage;
			}
			else if (navigationParameter != null)
			{
				if (navigationParameter is SelectSubredditMessage)
				{
					redditView.SubredditInfo = navigationParameter as SelectSubredditMessage;
				}
				else if (navigationParameter is string)
				{
					var navString = navigationParameter as string;
					var thing = JsonConvert.DeserializeObject<Thing>(navString);
					if (thing != null)
					{
						var link = thing.Data as Link;
						var subreddit = thing.Data as Subreddit;

						if (link != null)
						{
							var linkMessage = new NavigateToUrlMessage();
							linkMessage.TargetUrl = link.Url;
							linkMessage.Title = link.Title;
							Messenger.Default.Send<NavigateToUrlMessage>(linkMessage);
						}
						else if (subreddit != null)
						{
							var selectSubreddit = new SelectSubredditMessage();
							var typedSubreddit = new TypedThing<Subreddit>(new Thing { Kind = "t5", Data = subreddit });
							selectSubreddit.Subreddit = new TypedThing<Subreddit>(typedSubreddit);
							redditView.SubredditInfo = selectSubreddit;
						}
					}
				}
			}
			else
			{
				redditView.SubredditInfo = null;
			}
		}

		/// <summary>
		/// Preserves state associated with this page in case the application is suspended or the
		/// page is discarded from the navigation cache.  Values must conform to the serialization
		/// requirements of <see cref="SuspensionManager.SessionState"/>.
		/// </summary>
		/// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
		protected override void SaveState(Dictionary<String, Object> pageState)
		{
			pageState["SidebarState"] = _sidebarState;
			redditView.UpdateScrollOffset();
			pageState["ScrollOffset"] = redditView.ScrollOffset;
		}

		private void SubredditButton_Tapped(object sender, TappedRoutedEventArgs e)
		{
			var button = sender as Button;
			var subreddit = button.DataContext as AboutSubredditViewModel;
			if (subreddit != null)
			{
				if (redditView.Visibility == Windows.UI.Xaml.Visibility.Collapsed)
				{
					redditView.Visibility = Windows.UI.Xaml.Visibility.Visible;
					subredditPicker.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
				}
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
			subredditPicker.Visibility = Windows.UI.Xaml.Visibility.Visible;
			redditView.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
			return;
			App.SetSearchKeyboard(false);
			_redditPickerFlyout = new Flyout();
			_redditPickerFlyout.Width = 430;
			_redditPickerFlyout.Closed += (obj1, obj2) => App.SetSearchKeyboard(true);
			_redditPickerFlyout.Placement = PlacementMode.Right;
			_redditPickerFlyout.PlacementTarget = navigationView;
			_redditPickerFlyout.Content = new SubredditPickerView();
			_redditPickerFlyout.IsOpen = true;
		}

    }
}
