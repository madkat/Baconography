using GalaSoft.MvvmLight;
using SnooSharp;
using SnooStream.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnooStream.ViewModel
{
    public class SubredditRiverViewModel : ViewModelBase
    {
        public ObservableCollection<LinkRiverViewModel> PinnedRivers { get; private set; }
        public ObservableCollection<LinkRiverViewModel> SubscribedRivers { get; private set; }
        public SubredditRiverViewModel(SubredditRiverInit initBlob)
        {
            if (initBlob != null)
            {
                PinnedRivers = new ObservableCollection<LinkRiverViewModel>(initBlob.Pinned.Select(blob => new LinkRiverViewModel(blob.Thing, blob.DefaultSort, blob.Links)));
                SubscribedRivers = new ObservableCollection<LinkRiverViewModel>(initBlob.Subscribed.Select(blob => new LinkRiverViewModel(blob.Thing, blob.DefaultSort, blob.Links)));
                ReloadSubscribed(false);
            }
            else
            {
                LoadWithoutInitial();
            }
        }

        private async void LoadWithoutInitial()
        {
            PinnedRivers = new ObservableCollection<LinkRiverViewModel>();
            SubscribedRivers = new ObservableCollection<LinkRiverViewModel>();
            Listing subscribedListing = null; 
            if (SnooStreamViewModel.RedditUserState != null && !string.IsNullOrWhiteSpace(SnooStreamViewModel.RedditUserState.Username))
            {
                subscribedListing = await SnooStreamViewModel.RedditService.GetSubscribedSubredditListing() ?? await SnooStreamViewModel.RedditService.GetDefaultSubreddits();
            }
            else
            {
                subscribedListing = await SnooStreamViewModel.RedditService.GetDefaultSubreddits();
            }

            foreach (var river in subscribedListing.Data.Children.Select(thing => new LinkRiverViewModel(thing.Data as Subreddit, "hot", null)))
            {
                SubscribedRivers.Add(river);
            }
        }

        private async void ReloadSubscribed(bool required)
        {
            if (!required)
            {
                if (!SnooStreamViewModel.SystemServices.IsLowPriorityNetworkOk)
                    return;
                else
                    await Task.Delay(10000);
            }

            var subscribedListing = await SnooStreamViewModel.RedditService.GetSubscribedSubredditListing();

            foreach (var river in subscribedListing.Data.Children.Select(thing => new LinkRiverViewModel(thing.Data as Subreddit, "hot", null)))
            {
                SubscribedRivers.Add(river);
            }
        }
    }
}
