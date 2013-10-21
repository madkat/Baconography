using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using BaconographyWP8.View;

namespace BaconographyWP8Core.View
{
    public partial class RedditViewSingle : UserControl
    {
        public RedditViewSingle()
        {
            InitializeComponent();
            Loaded += RedditViewSingle_Loaded;
        }

        void RedditViewSingle_Loaded(object sender, RoutedEventArgs e)
        {
            CurrentRedditView = redditView;
        }



        public RedditView CurrentRedditView
        {
            get { return (RedditView)GetValue(CurrentRedditViewProperty); }
            set { SetValue(CurrentRedditViewProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentRedditViewProperty =
            DependencyProperty.Register("CurrentRedditViewProperty", typeof(RedditView), typeof(RedditViewSingle), new PropertyMetadata(null));

        
    }
}
