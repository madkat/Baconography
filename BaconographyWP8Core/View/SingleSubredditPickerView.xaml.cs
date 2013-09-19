using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using BaconographyWP8Core.ViewModel;
using BaconographyWP8.Common;
using BaconographyPortable.ViewModel;
using System.Windows.Data;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Practices.ServiceLocation;
using BaconographyPortable.Services;

namespace BaconographyWP8Core.View
{
    [ViewUri("/BaconographyWP8Core;component/View/SingleSubredditPickerView.xaml")]
    public partial class SingleSubredditPickerView : PhoneApplicationPage
    {
        SubredditPickerViewModel _spvm;
        public SingleSubredditPickerView()
        {
            InitializeComponent();
            _spvm = DataContext as SubredditPickerViewModel;
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
                        var viewModel = DataContext as SubredditPickerViewModel;
                        if (viewModel != null && viewModel.Subreddits.HasMoreItems)
                            viewModel.Subreddits.LoadMoreItemsAsync(30);
                    }
                }
            }

            var subredditVM = newListLastItem as AboutSubredditViewModel;
            if (subredditVM != null)
            {
                if (_spvm != null)
                {
                    var match = _spvm.SelectedSubreddits.FirstOrDefault<TypedSubreddit>(thing => thing.DisplayName == subredditVM.Thing.Data.DisplayName);
                    if (match != null)
                    {
                        subredditVM.Pinned = true;
                    }
                    else
                    {
                        subredditVM.Pinned = false;
                    }
                }
            }
        }

        private void manualBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                this.Focus();
                var ssvm = this.DataContext as SubredditSelectorViewModel;
                if (ssvm != null)
                    ssvm.PinSubreddit.Execute(ssvm);
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
            //((TextBox)sender).Focus();
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

        private void ChooseSubreddit(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var button = sender as Button;
            var subredditVM = button.DataContext as AboutSubredditViewModel;
            if (subredditVM != null)
            {
                if (_spvm != null)
                {
                    Messenger.Default.Send<PickedSubredditMessage>(new PickedSubredditMessage { SubredditName = subredditVM.Url.Replace("/r/", "").TrimEnd('/') });
                    ServiceLocator.Current.GetInstance<INavigationService>().GoBack();
                }
            }
        }
    }
}