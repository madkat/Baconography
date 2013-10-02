using BaconographyPortable.Messages;
using BaconographyPortable.Services;
using BaconographyPortable.ViewModel.Collections;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaconographyPortable.ViewModel
{
    public class MultipleRedditMainViewModel : SimpleMainViewModel
    {
        public MultipleRedditMainViewModel(IBaconProvider baconProvider) : base(baconProvider)
        {
            MessengerInstance.Register<SelectTemporaryRedditMessage>(this, OnSelectTemporarySubreddit);
            MessengerInstance.Register<CloseSubredditMessage>(this, OnCloseSubreddit);
            MessengerInstance.Register<ReorderSubredditMessage>(this, OnReorderSubreddit);
            MessengerInstance.Register<SettingsChangedMessage>(this, OnSettingsChanged);

            _pivotItems = new RedditViewModelCollection();
        }

        private RedditViewModelCollection _pivotItems;
        public RedditViewModelCollection PivotItems
        {
            get
            {
                return _pivotItems;
            }
        }

        private async void OnSettingsChanged(SettingsChangedMessage message)
        {
            if (!message.InitialLoad)
                await _baconProvider.GetService<ISettingsService>().Persist();
        }

        private void OnReorderSubreddit(ReorderSubredditMessage message)
        {
            if (PivotItems != null && Subreddits != null)
            {
                _suspendSaving = true;
                var redditVMs = PivotItems.Select(piv => piv is RedditViewModel ? piv as RedditViewModel : null).ToArray();
                for (int i = Subreddits.Count - 1; i >= 0; i--)
                {
                    if (redditVMs.Length > i && Subreddits[i].Data != null && redditVMs[i].Heading == Subreddits[i].Data.DisplayName)
                        continue;
                    else
                    {
                        var pivot = redditVMs.FirstOrDefault(rvm => Subreddits[i].Data != null && rvm.Heading == Subreddits[i].Data.DisplayName);
                        if (pivot != null)
                        {
                            PivotItems.Remove(pivot);
                            PivotItems.Insert(0, pivot);
                        }
                    }
                }
                _suspendSaving = false;
                _subreddits_CollectionChanged(null, null);
            }
        }

        private void OnCloseSubreddit(CloseSubredditMessage message)
        {
            string heading = message.Heading;
            if (message.Subreddit != null)
            {
                heading = message.Subreddit.Data.DisplayName;
            }

            if (!String.IsNullOrEmpty(heading))
            {

                var match = PivotItems.FirstOrDefault(vmb => vmb is RedditViewModel && (vmb as RedditViewModel).Heading == heading) as RedditViewModel;
                if (match != null)
                {
                    var subreddit = (match as RedditViewModel).SelectedSubreddit;
                    PivotItems.Remove(match);
                    if (!match.IsTemporary)
                    {
                        _subreddits.Remove(subreddit);
                    }
                }

            }
        }

        private void OnSelectTemporarySubreddit(SelectTemporaryRedditMessage message)
        {
            int indexToPosition;
            bool foundExisting = FindSubredditMessageIndex(message, out indexToPosition);

            if (!foundExisting)
            {
                var newReddit = new RedditViewModel(_baconProvider);
                newReddit.IsTemporary = true;
                newReddit.DetachSubredditMessage();
                newReddit.AssignSubreddit(message);
                if (PivotItems.Count > 0)
                    PivotItems.Insert(PivotItems.Count, newReddit);
                else
                    PivotItems.Add(newReddit);

                indexToPosition = PivotItems.Count - 1;
            }

            Messenger.Default.Send<SelectIndexMessage>(
                new SelectIndexMessage
                {
                    TypeContext = typeof(MultipleRedditMainViewModel),
                    Index = indexToPosition
                }
            );
        }

        public bool FindSubredditMessageIndex(SelectSubredditMessage message, out int indexToPosition)
        {
            indexToPosition = 0;
            foreach (var vm in PivotItems)
            {
                if (vm is RedditViewModel)
                {
                    if (((RedditViewModel)vm).Url == message.Subreddit.Data.Url)
                    {
                        return true;
                    }

                }
                indexToPosition++;
            }
            return false;
        }
    }
}
