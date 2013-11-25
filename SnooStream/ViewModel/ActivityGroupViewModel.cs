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
                return x.Created.CompareTo(y.Created);
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

        public DateTime Created {get; protected set;}
        public virtual ActivityViewModel FirstActivity 
        {
            get
            {
                return ((IEnumerable<ActivityViewModel>)Activities).First();
            }
        }
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
                Created = _linkActivity.Created;
            }
            else
            {
                var thingName = ((ThingData)additional.Data).Name;
                if (!Activities.ContainsKey(thingName))
                    Activities.Add(thingName, ActivityViewModel.CreateActivity(additional));
            }
            Created = FirstActivity.Created;
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
            if(!Activities.ContainsKey(thingName))
                Activities.Add(thingName, ActivityViewModel.CreateActivity(additional));

            Created = FirstActivity.Created;
        }
    }
}
