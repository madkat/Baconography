using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using SnooStream.ViewModel;
using GalaSoft.MvvmLight;

namespace SnooStreamWP8.View.Controls
{
    public partial class SubredditRiver : UserControl
    {
        public SubredditRiver()
        {
            InitializeComponent();
        }

        private void listBox_ItemTap(object sender, Telerik.Windows.Controls.ListBoxItemTapEventArgs e)
        {
            var linkRiver = e.Item.DataContext as LinkRiverViewModel;
            if (linkRiver != null)
                SnooStreamViewModel.NavigationService.NavigateToLinkRiver(linkRiver);
        }
    }
}
