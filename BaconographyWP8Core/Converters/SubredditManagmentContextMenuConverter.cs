using BaconographyPortable.Messages;
using BaconographyPortable.Model.Reddit;
using BaconographyPortable.Services;
using BaconographyPortable.ViewModel;
using BaconographyWP8Core.View;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Phone.Controls;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace BaconographyWP8.Converters
{
    public class SubredditManagmentContextMenuConverter : IValueConverter
    {
        private void _invalidateSubscribed()
        {

        }

        private void _invalidatePinned()
        {
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //offer details
            //if its pinned, offer to remove it
            //if not pinned offer to pin it
            //if its not subscribed offer to subscribe it, otherwise offer to remove it

            var aboutSubreddit = value as AboutSubredditViewModel;
            var reddit = value as RedditViewModel;

            List<MenuItem> menuItems = new List<MenuItem>();

            bool pinned = false;
            bool subscribed = false;
            TypedThing<Subreddit> thing = null;
            if(aboutSubreddit != null)
            {
                thing = aboutSubreddit.Thing;
            }
            else if(reddit != null)
            {
                thing = reddit.SelectedSubreddit;
            }

            var navigationService = ServiceLocator.Current.GetInstance<INavigationService>();
            var redditService = ServiceLocator.Current.GetInstance<IRedditService>();

            if (thing == null)
                return null;

            menuItems.Add(new MenuItem
            {
                Header = "About",
                Command = new RelayCommand(() =>
                    {
                        navigationService.Navigate(typeof(AboutSubreddit), new Tuple<string>(thing.Data.Url));
                        _invalidatePinned();
                    })
            });

            if (pinned)
            {
                menuItems.Add(new MenuItem
                {
                    Header = "Unpin",
                    Command = new RelayCommand(() =>
                        {
                            Messenger.Default.Send<CloseSubredditMessage>(new CloseSubredditMessage { Subreddit = thing });
                            _invalidatePinned();
                        })
                });
            }
            else
            {
                menuItems.Add(new MenuItem
                {
                    Header = "Pin",
                    Command = new RelayCommand(() =>
                    {
                        Messenger.Default.Send<SelectSubredditMessage>(new SelectSubredditMessage { Subreddit = thing });
                        _invalidatePinned();
                    })
                });
            }

            if (subscribed)
            {
                menuItems.Add(new MenuItem
                {
                    Header = "Unsubscribe",
                    Command = new RelayCommand(() =>
                        {
                            redditService.AddSubredditSubscription(thing.Data.Name, false);
                            _invalidateSubscribed();
                        })
                });
            }
            else
            {
                menuItems.Add(new MenuItem
                {
                    Header = "Subscribe",
                    Command = new RelayCommand(() =>
                    {
                        redditService.AddSubredditSubscription(thing.Data.Name, true);
                        _invalidateSubscribed();
                    })
                });
            }

            return menuItems;
            
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
