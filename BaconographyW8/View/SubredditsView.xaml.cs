using BaconographyPortable.Messages;
using BaconographyW8.Messages;
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

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace BaconographyW8.View
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class SubredditsView : BaconographyW8.Common.LayoutAwarePage
    {
        public SubredditsView()
        {
            this.InitializeComponent();
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
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary <String, Object> pageState)
        {
        }

		private void GridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (subredditGrid.SelectedItem != null)
			{
				var subreddit = subredditGrid.SelectedItem as AboutSubredditViewModel;
				Messenger.Default.Send<SelectSubredditMessage>(new SelectSubredditMessage { Subreddit = subreddit.Thing });
				Messenger.Default.Send<CloseSubredditsMessage>(new CloseSubredditsMessage());

				subredditGrid.SelectedItem = null;
				manualBox.Text = "";
			}
		}

		

		private void manualBox_KeyDown(object sender, KeyRoutedEventArgs e)
		{
		//	if (e.Key == Windows.System.VirtualKey.Enter)
		//	{
		//		this.Focus(Windows.UI.Xaml.FocusState.Pointer);
		//		var ssvm = this.DataContext as SubredditsViewModel;
		//		if (ssvm != null)
		//			ssvm.PinSubreddit.Execute(ssvm);
		//	}
		//	else
		//	{
		//		BindingExpression bindingExpression = ((TextBox)sender).GetBindingExpression(TextBox.TextProperty);
		//		if (bindingExpression != null)
		//		{
		//			bindingExpression.UpdateSource();
		//		}
		//	}
		}

		//this bit of unpleasantry is needed to prevent the input box from getting defocused when an item gets added to the collection
		bool _disableFocusHack = false;
		bool _needToHackFocus = false;
		TextBox _manualBox = null;
		private void manualBox_LostFocus(object sender, RoutedEventArgs e)
		{
			_manualBox = sender as TextBox;
			if (_disableFocusHack)
				_disableFocusHack = false;
			else
			{
				_needToHackFocus = true;
			}
			//((TextBox)sender).Focus();
		}

		private void manualBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			var vm = this.DataContext as SubredditsViewModel;
			if (vm != null)
			{
				vm.Text = manualBox.Text;
			}
		}

    }
}
