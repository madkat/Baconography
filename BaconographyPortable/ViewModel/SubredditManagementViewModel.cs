using BaconographyPortable.Messages;
using BaconographyPortable.Model.Reddit;
using BaconographyPortable.Model.Reddit.ListingHelpers;
using BaconographyPortable.Services;
using BaconographyPortable.ViewModel.Collections;
using GalaSoft.MvvmLight;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaconographyPortable.ViewModel
{
    public class SubredditManagementViewModel : ViewModelBase
    {
        ISystemServices _systemServices;
        IBaconProvider _baconProvider;
        public SubredditManagementViewModel(IBaconProvider baconProvider)
        {
            _baconProvider = baconProvider;
            _systemServices = baconProvider.GetService<ISystemServices>();
            MessengerInstance.Register<UserLoggedInMessage>(this, userLoggedIn);
            SearchSubNewGroup = new ObservableCollection<SubredditGroupBridge>();
            PlainSubreddits = new SubredditViewModelCollection(_baconProvider);
            PlainSubreddits.LoadMoreItemsAsync(50);
            SearchSubNewGroup.Add(new SubredditGroupBridge(PlainSubreddits, "Reddit", false));
        }

        private void userLoggedIn(UserLoggedInMessage obj)
        {
            _searchString = "";
            SearchSubNewGroup.Clear();
            if (MainViewModel != null && MainViewModel is MultipleRedditMainViewModel)
            {
                SearchSubNewGroup.Add(new SubredditGroupBridge(((MultipleRedditMainViewModel)MainViewModel).PivotItems, "Pivot", true));
            }
            if (MainViewModel != null)
            {
                SearchSubNewGroup.Add(new SubredditGroupBridge(MainViewModel.SubscribedSubreddits, "Subscribed", true));
            }

            SearchSubNewGroup.Add(new SubredditGroupBridge(PlainSubreddits, "Reddit", SearchSubNewGroup.Count > 0));
        }

        SimpleMainViewModel _mainViewModel;
        private SimpleMainViewModel MainViewModel
        {
            get
            {
                var viewModelLocator = _baconProvider.GetService<IViewModelLocator>();

                if (viewModelLocator.IsLoaded)
                {
                    if (_baconProvider.GetService<ISettingsService>().SimpleLayoutMode)
                        _mainViewModel = ServiceLocator.Current.GetInstance<SimpleMainViewModel>();
                    else
                        _mainViewModel = ServiceLocator.Current.GetInstance<MultipleRedditMainViewModel>();
                }
                return _mainViewModel;
            }
        }

        public class SubredditGroupBridge : ObservableCollection<ViewModelBase> 
        {
            public SubredditGroupBridge(IEnumerable<ViewModelBase> viewModels, string title, bool visible)
                : base(viewModels)
            {
                Title = title;
                Visible = visible;
            }

            public SubredditGroupBridge(ObservableCollection<ViewModelBase> viewModels, string title, bool visible)
                : base(viewModels)
            {
                Title = title;
                Visible = visible;
                viewModels.CollectionChanged += viewModels_CollectionChanged;
            }

            void viewModels_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
            {
                switch (e.Action)
                {
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                        Add(e.NewItems[0] as ViewModelBase);
                        break;
                }
            }
            public string Title { get; set; }
            public bool Visible { get; set; }
        }

        private SubredditViewModelCollection PlainSubreddits { get; set; }
        private SearchResultsViewModelCollection SearchResults { get; set; }
        public ObservableCollection<SubredditGroupBridge> SearchSubNewGroup { get; private set; }

        private string _searchString;
        public string SearchString
        {
            get
            {
                return _searchString;
            }
            set
            {
                bool wasChanged = _searchString != value;
                if (wasChanged)
                {
                    _searchString = value;
                    RaisePropertyChanged("TextSearchString");

                    if (_searchString.Length < 3)
                    {
                        SearchSubNewGroup.Clear();
                        if (MainViewModel != null && MainViewModel is MultipleRedditMainViewModel)
                        {
                            SearchSubNewGroup.Add(new SubredditGroupBridge(((MultipleRedditMainViewModel)MainViewModel).PivotItems, "Pivot", true));
                        }
                        if (MainViewModel != null)
                        {
                            SearchSubNewGroup.Add(new SubredditGroupBridge(MainViewModel.SubscribedSubreddits, "Subscribed", true));
                        }

                        SearchSubNewGroup.Add(new SubredditGroupBridge(PlainSubreddits, "Reddit", SearchSubNewGroup.Count > 0));
                        RevokeQueryTimer();
                    }
                    else
                    {
                        RestartQueryTimer();
                    }
                }
            }
        }

        Object _queryTimer;
        void RevokeQueryTimer()
        {
            if (_queryTimer != null)
            {
                _systemServices.StopTimer(_queryTimer);
                _queryTimer = null;
            }
        }

        void RestartQueryTimer()
        {
            // Start or reset a pending query
            if (_queryTimer == null)
            {
                _queryTimer = _systemServices.StartTimer(queryTimer_Tick, new TimeSpan(0, 0, 1), true);
            }
            else
            {
                _systemServices.StopTimer(_queryTimer);
                _systemServices.RestartTimer(_queryTimer);
            }
        }

        bool _searchLoadInProgress = false;

        async void queryTimer_Tick(object sender, object timer)
        {
            // Stop the timer so it doesn't fire again unless rescheduled
            RevokeQueryTimer();

            if (!(_searchString != null && _searchString.Contains("/")))
            {
                _searchLoadInProgress = true;
                try
                {
                    //get new search results
                    //filter out the pinned and subscribed subreddits in the search results
                    //if there are any pinned leave the pinned group visible otherwise clear it
                    //if there are any subscribed leave the sub group visible otherwise clear it
                    //add remining subreddits from the search results to the 3rd group
                    SearchResults = new SearchResultsViewModelCollection(_baconProvider, _searchString, true, null);
                    await SearchResults.LoadMoreItemsAsync(100);


                    var pivotGroup = new SubredditGroupBridge(new ViewModelBase[0], "Pivot", true);
                    var subscribedGroup = new SubredditGroupBridge(new ViewModelBase[0], "Subscribed", true);
                    var searchGroup = new SubredditGroupBridge(new ViewModelBase[0], "Search", true);

                    foreach (var item in SearchResults)
                    {
                        var subredditViewModel = item as AboutSubredditViewModel;
                        if (subredditViewModel.Pinned)
                        {
                            pivotGroup.Add(subredditViewModel);
                        }
                        else if (subredditViewModel.Subscribed)
                        {
                            subscribedGroup.Add(subredditViewModel);
                        }
                        else
                        {
                            searchGroup.Add(subredditViewModel);
                        }
                    }

                    if (pivotGroup.Count == 0 && subscribedGroup.Count == 0)
                    {
                        searchGroup.Visible = false;
                    }

                    if (pivotGroup.Count == 0)
                    {
                        pivotGroup.Visible = false;
                    }

                    if (subscribedGroup.Count == 0)
                    {
                        subscribedGroup.Visible = false;
                    }

                    SearchSubNewGroup.Add(pivotGroup);
                    SearchSubNewGroup.Add(subscribedGroup);
                    SearchSubNewGroup.Add(searchGroup);

                }
                finally
                {
                    _searchLoadInProgress = false;
                }
            }
        }
        
    }
}
