using GalaSoft.MvvmLight;
using SnooSharp;
using SnooStream.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnooStream.ViewModel
{
    public class SelfViewModel : ViewModelBase
    {
        public SelfViewModel(IEnumerable<Thing> initialThings, string oldestMessage, string oldestSentMessage, string oldestActivity)
        {
            //load up the activities
            Activities = new ObservableSortedUniqueCollection<string, ActivityGroupViewModel>(new ActivityGroupViewModel.ActivityAgeComparitor());
            foreach (var thing in initialThings)
            {
                var thingName = ActivityViewModel.GetActivityGroupName(thing);
                ActivityGroupViewModel existingGroup;
                if (Activities.TryGetValue(thingName, out existingGroup))
                {
                    existingGroup.Merge(thing);
                }
                else
                {
                    Activities.Add(thingName, ActivityGroupViewModel.MakeActivityGroup(thing));
                }
            }

            OldestMessage = oldestMessage;
            OldestSentMessage = oldestSentMessage;
            OldestActivity = oldestActivity;
        }

        

        private string OldestMessage { get; set; }
        private string OldestSentMessage { get; set; }
        private string OldestActivity { get; set; }
        public ObservableSortedUniqueCollection<string, ActivityGroupViewModel> Activities { get; private set; }
        public async Task PullNew()
        {
            Listing inbox = null;
            Listing outbox = null;
            Listing activity = null;

            await SnooStreamViewModel.NotificationService.Report("refreshing inbox", async () =>
                {
                    inbox = await SnooStreamViewModel.RedditService.GetMessages(null);
                });

            OldestMessage = ProcessListing(inbox, OldestMessage);

            await SnooStreamViewModel.NotificationService.Report("refreshing outbox", async () =>
                {
                    outbox = await SnooStreamViewModel.RedditService.GetSentMessages(null);
                });

            OldestSentMessage = ProcessListing(inbox, OldestSentMessage);

            await SnooStreamViewModel.NotificationService.Report("refreshing activity", async () =>
                {
                    activity = await SnooStreamViewModel.RedditService.GetPostsByUser(SnooStreamViewModel.RedditService.CurrentUserName, null);
                });

            OldestActivity = ProcessListing(inbox, OldestActivity);
        }

        private string ProcessListing(Listing listing, string after)
        {
            if (listing != null)
            {
                foreach (var child in listing.Data.Children)
                {
                    var childName = ActivityViewModel.GetActivityGroupName(child);
                    ActivityGroupViewModel existingGroup;
                    if (Activities.TryGetValue(childName, out existingGroup))
                    {
                        existingGroup.Merge(child);
                    }
                    else
                    {
                        Activities.Add(childName, ActivityGroupViewModel.MakeActivityGroup(child));
                    }
                }

                if (string.IsNullOrWhiteSpace(after))
                    return listing.Data.After;
            }
            return after;
        }

        public async Task PullOlder()
        {
            Listing inbox = null;
            Listing outbox = null;
            Listing activity = null;

            await SnooStreamViewModel.NotificationService.Report("getting additional inbox", async () =>
            {
                inbox = await SnooStreamViewModel.RedditService.GetAdditionalFromListing(string.Format(Reddit.MailBaseUrlFormat, "inbox"), OldestMessage, null);
            });

            OldestMessage = ProcessListing(inbox, OldestMessage);

            await SnooStreamViewModel.NotificationService.Report("getting additional outbox", async () =>
            {
                outbox = await SnooStreamViewModel.RedditService.GetAdditionalFromListing(string.Format(Reddit.MailBaseUrlFormat, "sent"), OldestSentMessage, null);
            });

            OldestSentMessage = ProcessListing(inbox, OldestSentMessage);

            await SnooStreamViewModel.NotificationService.Report("getting additional activity", async () =>
            {
                activity = await SnooStreamViewModel.RedditService.GetAdditionalFromListing(string.Format(Reddit.PostByUserBaseFormat, SnooStreamViewModel.RedditService.CurrentUserName), OldestActivity, null);
            });

            OldestActivity = ProcessListing(inbox, OldestActivity);
        }
    }
}
