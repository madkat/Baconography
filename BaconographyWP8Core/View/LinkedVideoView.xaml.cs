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

namespace BaconographyWP8Core.View
{
    [ViewUri("/BaconographyWP8Core;component/View/LinkedVideoView.xaml")]
    public partial class LinkedVideoView : PhoneApplicationPage
    {
        public LinkedVideoView()
        {
            InitializeComponent();
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.New && e.Uri.ToString() == "/BaconographyWP8Core;component/MainPage.xaml" && e.IsCancelable)
            {
                e.Cancel = true;
            }
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
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
                if (e.NavigationMode == NavigationMode.New && e.IsNavigationInitiator)
                {

                    var absPath = e.Uri.ToString().Contains('?') ? e.Uri.ToString().Substring(0, e.Uri.ToString().IndexOf("?")) : e.Uri.ToString();
                    if (absPath == "/BaconographyWP8Core;component/View/LinkedPictureView.xaml" || absPath == "/BaconographyWP8Core;component/View/LinkedReadabilityView.xaml" ||
                        absPath == "/BaconographyWP8Core;component/View/LinkedSelfTextPageView.xaml" || absPath == "/BaconographyWP8Core;component/View/LinkedVideoView.xaml")
                    {
                        ServiceLocator.Current.GetInstance<INavigationService>().RemoveBackEntry();
                    }
                }

                if (SimpleIoc.Default.IsRegistered<WebVideoViewModel>())
                {
                    var preloadedDataContext = SimpleIoc.Default.GetInstance<WebVideoViewModel>();
                    DataContext = preloadedDataContext;
                    SimpleIoc.Default.Unregister<WebVideoViewModel>();
                }
            }
        }

        public void myGridGestureListener_Flick(object sender, FlickGestureEventArgs e)
        {
            FlipViewUtility.FlickHandler(sender, e, DataContext as ViewModelBase, this);
        }
    }
}