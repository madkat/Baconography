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
using BaconographyWP8.Common;
using BaconographyPortable.Model.Reddit;
using GalaSoft.MvvmLight.Messaging;
using BaconographyWP8.Converters;

namespace BaconographyWP8Core.View
{
    [ViewUri("/BaconographyWP8Core;component/View/SimpleSubredditManagerView.xaml")]
    public partial class SimpleSubredditManagerView : PhoneApplicationPage
    {
        public SimpleSubredditManagerView()
        {
            InitializeComponent();
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.New && e.Uri.ToString() == "/BaconographyWP8Core;component/MainPage.xaml" && e.IsCancelable)
                e.Cancel = true;
            else
                base.OnNavigatingFrom(e);
        }
    }
}