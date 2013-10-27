using BaconographyPortable.Messages;
using BaconographyPortable.Services;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaconographyPortable.ViewModel.Collections
{
    public class SubscribedSubredditViewModelCollection : ThingViewModelCollection
    {
        public SubscribedSubredditViewModelCollection(IBaconProvider baconProvider)
            : base(baconProvider,
                new BaconographyPortable.Model.Reddit.ListingHelpers.SubredditSubscriptions(baconProvider),
                new BaconographyPortable.Model.KitaroDB.ListingHelpers.SubredditSubscriptions(baconProvider)) 
        {
            Messenger.Default.Register<SubredditSubscriptionChangeMessage>(this, onSubscriptionChanged);
        }

        private void onSubscriptionChanged(SubredditSubscriptionChangeMessage obj)
        {
            foreach (var item in this)
            {
                if (((AboutSubredditViewModel)item).Url == obj.ChangedUrl)
                {
                    //it was already here just ignore the request
                    if (obj.Added)
                    {
                        return;
                    }
                    else
                    {
                        this.Remove(item);
                        return;
                    }
                }
            }

            if (obj.Added && obj.ViewModel != null)
            {
                this.Add(obj.ViewModel);
            }
        }
    }
}
