using GalaSoft.MvvmLight;
using SnooSharp;
using SnooStream.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace SnooStream.ViewModel
{
    public abstract class ActivityGroupViewModel : ViewModelBase
    {
        public class ActivityAgeComparitor : IComparer<ActivityGroupViewModel>
        {
            public int Compare(ActivityGroupViewModel x, ActivityGroupViewModel y)
            {
                //invert the sort
                return y.CreatedUTC.CompareTo(x.CreatedUTC);
            }
        }

        public static ActivityGroupViewModel MakeActivityGroup(Thing thing)
        {
            if (thing == null)
                throw new ArgumentNullException();

            ActivityGroupViewModel group;
            if (thing.Data is Link)
            {
                group = new LinkGroupActivityViewModel();
            }
            else
            {
                group = new SimpleGroupActivityViewModel();
            }

            group.Merge(thing);
            return group;
        }

        public abstract void Merge(Thing additional);

        public DateTime CreatedUTC {get; protected set;}
        public virtual ActivityViewModel FirstActivity 
        {
            get
            {
                return ((IEnumerable<ActivityViewModel>)Activities).First();
            }
        }

        public bool IsConversation { get; protected set; }
        public ObservableSortedUniqueCollection<string, ActivityViewModel> Activities { get; protected set; }
    }

    public class LinkGroupActivityViewModel : ActivityGroupViewModel
    {

        public LinkGroupActivityViewModel()
        {
            Activities = new ObservableSortedUniqueCollection<string, ActivityViewModel>(new ActivityViewModel.ActivityAgeComparitor());
        }

        ActivityViewModel _linkActivity;
        public override void Merge(Thing additional)
        {
            if (additional.Data is Link)
            {
                _linkActivity = ActivityViewModel.CreateActivity(additional);
                CreatedUTC = _linkActivity.CreatedUTC;
            }
            else
            {
                var thingName = ((ThingData)additional.Data).Name;
                if (!Activities.ContainsKey(thingName))
                {
                    Activities.Add(thingName, ActivityViewModel.CreateActivity(additional));

                    if (!IsConversation)
                    {
                        IsConversation = true;
                        RaisePropertyChanged("IsConversation");
                    }
                }
            }
            CreatedUTC = FirstActivity.CreatedUTC;
        }

        public override ActivityViewModel FirstActivity
        {
            get
            {
                return _linkActivity;
            }
        }
    }

    public class SimpleGroupActivityViewModel : ActivityGroupViewModel
    {
        public SimpleGroupActivityViewModel()
        {
            Activities = new ObservableSortedUniqueCollection<string, ActivityViewModel>(new ActivityViewModel.ActivityAgeComparitor());
        }

        public override void Merge(Thing additional)
        {
            var thingName = ((ThingData)additional.Data).Name;
            if (!Activities.ContainsKey(thingName))
            {
                Activities.Add(thingName, ActivityViewModel.CreateActivity(additional));
                if (!IsConversation && Activities.Count > 0)
                {
                    IsConversation = true;
                    RaisePropertyChanged("IsConversation");
                }
            }

            CreatedUTC = FirstActivity.CreatedUTC;
        }
    }
}
