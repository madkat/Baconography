using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using Newtonsoft.Json;
using Microsoft.Practices.ServiceLocation;
using BaconographyPortable.Services;
using BaconographyPortable.Model.Reddit;
using BaconographyPortable.ViewModel;
using GalaSoft.MvvmLight.Messaging;
using BaconographyPortable.Messages;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace BaconographyW8.View
{
    public partial class AboutSubreddit : UserControl
    {
        public AboutSubreddit()
        {
            InitializeComponent();
        }

        protected async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back)
            {

            }
            else
            {
				/*
                if (this.NavigationContext.QueryString["data"] != null)
                {
                    try
                    {
                        var unescapedData = HttpUtility.UrlDecode(this.NavigationContext.QueryString["data"]);
                        var deserializedObject = JsonConvert.DeserializeObject<Tuple<string>>(unescapedData);
                        var redditService = ServiceLocator.Current.GetInstance<IRedditService>();
                        var subredditName = deserializedObject.Item1;
                        if (subredditName.Contains("/r/"))
                        {
                            subredditName = subredditName.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries)[1];
                        }

                        displayNameTextBlock.Text = subredditName;

                        Messenger.Default.Send<LoadingMessage>(new LoadingMessage { Loading = true });
                        try
                        {
                            var targetSubredditThing = await redditService.GetSubreddit(subredditName);
                            if (targetSubredditThing != null)
                            {
                                var sublist = await redditService.GetSubscribedSubreddits();
                                var viewModel = new AboutSubredditViewModel(ServiceLocator.Current.GetInstance<IBaconProvider>(), targetSubredditThing, sublist.Contains(targetSubredditThing.Data.Name));
                                DataContext = viewModel;
                                ContentPanel.Visibility = Visibility.Visible;
                            }
                        }
                        finally
                        {
                            Messenger.Default.Send<LoadingMessage>(new LoadingMessage { Loading = false });
                        }
                    }
                    catch (Exception ex)
                    {
                        ServiceLocator.Current.GetInstance<INotificationService>().CreateNotification("failed to display subreddit sidebar: " + ex.ToString());
                    }
                }
				 * */

            }
        }
    }
}