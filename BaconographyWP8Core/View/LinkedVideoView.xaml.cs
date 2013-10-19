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
                if (SimpleIoc.Default.IsRegistered<ReadableArticleViewModel>())
                {
                    var preloadedDataContext = SimpleIoc.Default.GetInstance<WebVideoViewModel>();
                    DataContext = preloadedDataContext;
                    SimpleIoc.Default.Unregister<ReadableArticleViewModel>();
                }
            }
        }
    }
}