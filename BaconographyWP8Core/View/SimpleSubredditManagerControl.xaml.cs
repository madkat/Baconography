using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using BaconographyWP8.Common;
using BaconographyPortable.ViewModel;
using BaconographyPortable.Model.Reddit;
using Microsoft.Practices.ServiceLocation;
using BaconographyPortable.Services;
using System.Windows.Data;

namespace BaconographyWP8Core.View
{
    public partial class SimpleSubredditManagerControl : UserControl
    {
        public SimpleSubredditManagerControl()
        {
            InitializeComponent();
        }

        const int _offsetKnob = 7;
        private object newListLastItem;


        void newList_ItemRealized(object sender, ItemRealizationEventArgs e)
        {
            newListLastItem = e.Container.Content;
            var linksView = sender as FixedLongListSelector;
            if (linksView.ItemsSource != null && linksView.ItemsSource.Count >= _offsetKnob)
            {
                if (e.ItemKind == LongListSelectorItemKind.Item)
                {
                    if ((e.Container.Content).Equals(linksView.ItemsSource[linksView.ItemsSource.Count - _offsetKnob]))
                    {
                        if (DataContext is SubredditManagementViewModel)
                        {
                            ((SubredditManagementViewModel)DataContext).LoadMore();
                        }
                    }
                }
            }
        }

        private void manualBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                this.Focus();
                var ssvm = this.DataContext as SubredditManagementViewModel;
                if (ssvm != null)
                    ssvm.DoGoSubreddit(ssvm.SearchString, true);
            }
            else
            {
                BindingExpression bindingExpression = ((TextBox)sender).GetBindingExpression(TextBox.TextProperty);
                if (bindingExpression != null)
                {
                    bindingExpression.UpdateSource();
                }
            }
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
        }

        private void manualBox_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _disableFocusHack = true;
            _needToHackFocus = false;
        }

        private void FixedLongListSelector_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!_disableFocusHack && _needToHackFocus)
            {
                _needToHackFocus = false;
                _manualBox.Focus();
            }
        }

        private void FixedLongListSelector_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            TypedThing<Subreddit> targetSubreddit = null;
            var senderElement = e.OriginalSource as FrameworkElement;
            if (senderElement != null)
            {
                var context = senderElement.DataContext as AboutSubredditViewModel;
                if (context != null)
                {
                    targetSubreddit = context.Thing;
                }
                else if (senderElement.DataContext is RedditViewModel)
                {
                    targetSubreddit = ((RedditViewModel)senderElement.DataContext).SelectedSubreddit;
                }
            }

            if (targetSubreddit != null)
            {
                ((SubredditManagementViewModel)DataContext).DoGoSubreddit(targetSubreddit, true);
                if(!ServiceLocator.Current.GetInstance<ISettingsService>().SimpleLayoutMode)
                    ServiceLocator.Current.GetInstance<INavigationService>().GoBack();
            }
        }
    }
}
