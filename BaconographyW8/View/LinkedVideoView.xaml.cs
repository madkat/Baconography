﻿using BaconographyPortable.ViewModel;
using GalaSoft.MvvmLight.Ioc;
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
    public sealed partial class LinkedVideoView : BaconographyW8.Common.LayoutAwarePage
    {
        IEnumerable<Dictionary<string, string>> _navParam;
        public LinkedVideoView()
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
            _navParam = navigationParameter as IEnumerable<Dictionary<string, string>>;
            if (_navParam == null && pageState != null && pageState.ContainsKey("NavParam"))
            {
                _navParam = pageState["NavParam"] as IEnumerable<Dictionary<string, string>>;
            }

            if (_navParam is IEnumerable<Dictionary<string, string>>)
            {
				if (SimpleIoc.Default.IsRegistered<WebVideoViewModel>())
				{
					var preloadedDataContext = SimpleIoc.Default.GetInstance<WebVideoViewModel>();
					DataContext = preloadedDataContext;
					SimpleIoc.Default.Unregister<WebVideoViewModel>();
				}
            }
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
            pageState["NavParam"] = _navParam;
        }
    }
}