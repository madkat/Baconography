using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Practices.ServiceLocation;
using BaconographyPortable.Services;
using BaconographyPortable.Messages;
using BaconographyWP8Core.ViewModel;
using System.Windows.Data;
using BaconographyPortable.ViewModel;

namespace BaconographyWP8Core.View
{
    [ViewUri("/BaconographyWP8Core;component/View/SimpleSubredditManagerView.xaml")]
    public partial class SimpleSubredditManagerView : PhoneApplicationPage
    {
        public SimpleSubredditManagerView()
        {
            InitializeComponent();
        }
        const int _offsetKnob = 7;
        private object newListLastItem;
        private object subbedListLastItem;

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.New && e.Uri.ToString() == "/BaconographyWP8Core;component/MainPage.xaml" && e.IsCancelable)
                e.Cancel = true;
            else
                base.OnNavigatingFrom(e);
        }
        private void Subreddit_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Subreddit_Hold(object sender, System.Windows.Input.GestureEventArgs e)
        {

        }

        void newList_ItemRealized(object sender, ItemRealizationEventArgs e)
        {
        }

        private void manualBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                this.Focus();
                var ssvm = this.DataContext as SubredditManagementViewModel;
                //if (ssvm != null)
                //    ssvm.PinSubreddit.Execute(ssvm);
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
    }
}