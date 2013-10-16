using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using BaconographyPortable.ViewModel;
using Microsoft.Practices.ServiceLocation;
using BaconographyPortable.Services;
using GalaSoft.MvvmLight.Messaging;
using BaconographyPortable.Messages;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using Newtonsoft.Json;
using System.Threading.Tasks;
using BaconographyW8.Common;

namespace BaconographyW8.View
{
    public partial class LinkedReadabilityView : LayoutAwarePage
    {
        public LinkedReadabilityView()
        {
            InitializeComponent();
        }

        private void DeFocusContent()
        {
            var context = DataContext as ReadableArticleViewModel;
            if (context != null)
                context.ContentIsFocused = false;

            articleView.IsHitTestVisible = false;
        }

        private void FocusContent()
        {
            var context = DataContext as ReadableArticleViewModel;
            if (context != null)
                context.ContentIsFocused = true;

            articleView.IsHitTestVisible = true;
        }


		Tuple<string, string> _argTuple;

		/// <summary>
		/// Populates the page with content passed during navigation.  Any saved state is also
		/// provided when recreating a page from a prior session.
		/// </summary>
		/// <param name="navigationParameter">The parameter value passed to
		/// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
		/// </param>
		/// <param name="pageState">A dictionary of state preserved by this page during an earlier
		/// session.  This will be null the first time a page is visited.</param>
		protected override async void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
		{
			if (SimpleIoc.Default.IsRegistered<ReadableArticleViewModel>())
			{
				var preloadedDataContext = SimpleIoc.Default.GetInstance<ReadableArticleViewModel>();
				DataContext = preloadedDataContext;
				SimpleIoc.Default.Unregister<ReadableArticleViewModel>();

				if (preloadedDataContext.ContentIsFocused)
					FocusContent();
				else
					DeFocusContent();
			}
			else if (pageState != null && pageState.ContainsKey("ArgData") && pageState["ArgData"] != null)
			{
				var unescapedData = pageState["ArgData"] as string;
				_argTuple = JsonConvert.DeserializeObject<Tuple<string, string>>(unescapedData);
			}
			else if (navigationParameter is Tuple<string, string>)
			{
				_argTuple = navigationParameter as Tuple<string, string>;
			}

			if (_argTuple != null)
			{
				try
				{
					Messenger.Default.Send<LoadingMessage>(new LoadingMessage { Loading = true });
					try
					{
						DataContext = await ReadableArticleViewModel.LoadAtLeastOne(ServiceLocator.Current.GetInstance<ISimpleHttpService>(), _argTuple.Item1, _argTuple.Item2);
						FocusContent();
					}
					catch (Exception ex)
					{
						ServiceLocator.Current.GetInstance<INavigationService>().GoBack();
						if (Uri.IsWellFormedUriString(_argTuple.Item1, UriKind.Absolute))
							ServiceLocator.Current.GetInstance<INavigationService>().NavigateToExternalUri(new Uri(_argTuple.Item1));

					}
				}
				finally
				{
					Messenger.Default.Send<LoadingMessage>(new LoadingMessage { Loading = false });
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
			pageState["ArgData"] = _argTuple.ToString();
		}

        private bool ContentFocused
        {
            get
            {
                var context = DataContext as ReadableArticleViewModel;
                if (context != null)
                    return context.ContentIsFocused;
                else
                    return false;
            }
        }

    }
}