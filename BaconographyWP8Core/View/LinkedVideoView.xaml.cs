using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using GalaSoft.MvvmLight.Ioc;
using BaconographyPortable.ViewModel;
using Microsoft.Practices.ServiceLocation;
using BaconographyPortable.Services;
using BaconographyWP8Core.Common;
using GalaSoft.MvvmLight;
using System.Windows.Threading;
using System.Windows.Media.Animation;
using System.Windows.Media;
using BaconographyWP8.Messages;
using GalaSoft.MvvmLight.Messaging;

namespace BaconographyWP8Core.View
{
    [ViewUri("/BaconographyWP8Core;component/View/LinkedVideoView.xaml")]
    public partial class LinkedVideoView : PhoneApplicationPage
    {
        public LinkedVideoView()
        {
            InitializeComponent();
        }

        void myGridGestureListener_Handle(object sender, Microsoft.Phone.Controls.GestureEventArgs e)
        {
            appBar.Interact();
        }

        private void AdjustForOrientation(PageOrientation orientation)
        {
            Messenger.Default.Send<OrientationChangedMessage>(new OrientationChangedMessage { Orientation = orientation });
            lastKnownOrientation = orientation;

            if (player != null)
            {
                if (orientation == PageOrientation.LandscapeRight)
                    player.Margin = new Thickness(0, 0, 60, 0);
                else if (orientation == PageOrientation.LandscapeLeft)
                    player.Margin = new Thickness(0, 0, 60, 0);
                else
                    player.Margin = new Thickness(0, 0, 0, 90);
            }
        }

        public void myGridGestureListener_Flick(object sender, FlickGestureEventArgs e)
        {
            FlipViewUtility.FlickHandler(sender, e, DataContext as ViewModelBase, this);
        }

        PageOrientation lastKnownOrientation;

        protected override void OnOrientationChanged(OrientationChangedEventArgs e)
        {
            AdjustForOrientation(e.Orientation);
            base.OnOrientationChanged(e);
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.New && e.Uri.ToString() == "/BaconographyWP8Core;component/MainPage.xaml" && e.IsCancelable)
            {
                e.Cancel = true;
            }
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.New && e.IsNavigationInitiator)
            {

                var absPath = e.Uri.ToString().Contains('?') ? e.Uri.ToString().Substring(0, e.Uri.ToString().IndexOf("?")) : e.Uri.ToString();
                if (absPath == "/BaconographyWP8Core;component/View/LinkedPictureView.xaml" || absPath == "/BaconographyWP8Core;component/View/LinkedReadabilityView.xaml" ||
                    absPath == "/BaconographyWP8Core;component/View/LinkedSelfTextPageView.xaml" || absPath == "/BaconographyWP8Core;component/View/LinkedVideoView.xaml")
                {
                    ServiceLocator.Current.GetInstance<INavigationService>().RemoveBackEntry();
                }
            }
        }
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            this.AdjustForOrientation(this.Orientation);
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.Back)
            {

            }
            else if (e.NavigationMode == NavigationMode.Reset)
            {
                //do nothing we have everything we want already here
            }
            else
            {
                if (SimpleIoc.Default.IsRegistered<WebVideoViewModel>())
                {
                    var preloadedDataContext = SimpleIoc.Default.GetInstance<WebVideoViewModel>();
                    DataContext = preloadedDataContext;
                    SimpleIoc.Default.Unregister<WebVideoViewModel>();
                }
            }
        }

    }
}