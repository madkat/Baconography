﻿using BaconographyPortable.Messages;
using BaconographyPortable.Model.Reddit;
using BaconographyPortable.Services;
using BaconographyPortable.ViewModel.Collections;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaconographyPortable.ViewModel
{
    public class SimpleMainViewModel : ViewModelBase
    {
        protected IBaconProvider _baconProvider;
        protected IRedditService _redditService;
        protected IDynamicViewLocator _dynamicViewLocator;
        protected INavigationService _navigationService;
        protected IUserService _userService;
        protected ILiveTileService _liveTileService;
        protected IOfflineService _offlineService;
        protected ISettingsService _settingsService;
		protected INotificationService _notificationService;

		protected bool _initialLoad = true;
        protected bool _suspendSaving = false;
        protected bool _currentlySavingSubreddits = false;
		protected IViewModelLocator _viewModelLocator;

        public SimpleMainViewModel(IBaconProvider baconProvider)
        {
            _baconProvider = baconProvider;
            _redditService = baconProvider.GetService<IRedditService>();
            _dynamicViewLocator = baconProvider.GetService<IDynamicViewLocator>();
            _navigationService = baconProvider.GetService<INavigationService>();
            _userService = baconProvider.GetService<IUserService>();
            _liveTileService = baconProvider.GetService<ILiveTileService>();
            _offlineService = baconProvider.GetService<IOfflineService>();
            _settingsService = baconProvider.GetService<ISettingsService>();
			_notificationService = baconProvider.GetService<INotificationService>();

			_subreddits = new ObservableCollection<TypedThing<Subreddit>>();
			_displayedSubreddits = new ObservableCollection<AboutSubredditViewModel>();
			_viewModelLocator = baconProvider.GetService<IViewModelLocator>();

			//MessengerInstance.Send<UserLoggedInMessage>(new UserLoggedInMessage { CurrentUser = _userService.GetUser().Result, UserTriggered = false });
        }

        public event Action InitialItemsLoaded;

		private Subreddit _currentSubreddit;
		public Subreddit CurrentSubreddit
		{
			get
			{
                if (_viewModelLocator.Reddit != null && _viewModelLocator.Reddit.SelectedSubreddit != null)
                    return _viewModelLocator.Reddit.SelectedSubreddit.Data;
                else if (_subreddits != null && _subreddits.Count() > 0)
                    return _subreddits.FirstOrDefault().Data;
				return null;
			}
		}

        public RedditViewModel CurrentViewModel
        {
            get
            {
                if (_viewModelLocator.Reddit != null && _viewModelLocator.Reddit.SelectedSubreddit != null)
                    return _viewModelLocator.Reddit;
                return null;
            }
        }

		private void CheckSelections()
		{
			foreach (var sub in _displayedSubreddits)
			{
                if (CurrentSubreddit != null && sub.DisplayName == CurrentSubreddit.DisplayName)
                {
                    sub.Selected = true;
                }
                else
                {
                    if (sub.Selected)
                        sub.Selected = false;
                }
			}
		}

		protected virtual void OnSubredditChanged(SelectSubredditMessage message)
		{
			CheckSelections();
		}

		private async void OnUserLoggedIn(UserLoggedInMessage message)
		{
			LoggedIn = message.CurrentUser != null && !string.IsNullOrWhiteSpace(message.CurrentUser.LoginCookie);

			if (_initialLoad)
			{
				await LoadSubreddits();
				_initialLoad = false;
			}

            if (LoggedIn)
			    SubscribedSubreddits.Refresh();
		}

		protected async void _subreddits_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			if (_suspendSaving)
				return;

			int retryCount = 0;
			while (_currentlySavingSubreddits && retryCount++ < 10)
			{
				await Task.Delay(100);
			}

			if (_currentlySavingSubreddits)
				return;

			try
			{
				_currentlySavingSubreddits = true;
				await SaveSubreddits();
			}
			catch { }
			finally
			{
				_currentlySavingSubreddits = false;
			}
		}

		public virtual async Task SaveSubreddits()
		{
			try
			{
				await _offlineService.StoreOrderedThings("subredditlist", Subreddits);
			}
			catch { }

		}

        protected void FireInitialItemsLoaded()
        {
            if (InitialItemsLoaded != null)
                InitialItemsLoaded();
        }

		public virtual async Task LoadSubreddits()
		{
			try
			{
				var subreddits = await _offlineService.RetrieveOrderedThings("subredditlist", TimeSpan.FromDays(1024));

				if (subreddits == null || subreddits.Count() == 0)
					subreddits = new List<TypedThing<Subreddit>> { new TypedThing<Subreddit>(ThingUtility.GetFrontPageThing()) };

				foreach (var sub in subreddits)
				{
					if (sub.Data is Subreddit && (((Subreddit)sub.Data).Id != null || ((Subreddit)sub.Data).Url.Contains("/m/")))
					{
						_subreddits.Add(new TypedThing<Subreddit>(sub));
					}
				}

                var newReddit = new RedditViewModel(_baconProvider);
                newReddit.AssignSubreddit(new SelectSubredditMessage { Subreddit = _subreddits.First() });

                MessengerInstance.Send<SubredditFocusedMessage>(new SubredditFocusedMessage { SelectedViewModel = newReddit });

				RefreshDisplaySubreddits();
				RaisePropertyChanged("Subreddits");

				_subreddits.CollectionChanged += _subreddits_CollectionChanged;
                FireInitialItemsLoaded();
			}
			catch
			{
				_notificationService.CreateNotification("Failed loading subreddits list, file corruption may be present");
			}
		}

		public void RefreshDisplaySubreddits()
		{
			_displayedSubreddits.Clear();

			foreach (var sub in _subreddits)
			{
				_displayedSubreddits.Add(new AboutSubredditViewModel(_baconProvider, sub, false));
			}

			foreach (var sub in SubscribedSubreddits)
			{
				var temp = sub as AboutSubredditViewModel;
				if (temp != null)
					_displayedSubreddits.Add(temp);
			}

			CheckSelections();

			RaisePropertyChanged("DisplayedSubreddits");
		}

		private bool _loggedIn;
		public bool LoggedIn
		{
			get
			{
				return _loggedIn;
			}
			set
			{
				_loggedIn = value;
				try
				{
					RaisePropertyChanged("LoggedIn");
				}
				catch { }
			}
		}

		public ObservableCollection<TypedThing<Subreddit>> _subreddits;
		public ObservableCollection<TypedThing<Subreddit>> Subreddits
		{
			get
			{
				return _subreddits;
			}
			set
			{
				_subreddits = value;
				RaisePropertyChanged("Subreddits");
			}
		}

		public ObservableCollection<AboutSubredditViewModel> _displayedSubreddits;
		public ObservableCollection<AboutSubredditViewModel> DisplayedSubreddits
		{
			get
			{
				return _displayedSubreddits;
			}
			set
			{
				_displayedSubreddits = value;
				RaisePropertyChanged("DisplayedSubreddits");
			}
		}

		private SubscribedSubredditViewModelCollection _subscribedSubreddits;
		public SubscribedSubredditViewModelCollection SubscribedSubreddits
		{
			get
			{
				if (_subscribedSubreddits == null)
				{
					_subscribedSubreddits = new SubscribedSubredditViewModelCollection(_baconProvider);
					_subscribedSubreddits.CollectionChanged += _subscribedSubreddits_CollectionChanged;
				}
				return _subscribedSubreddits;
			}
		}

		void _subscribedSubreddits_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			RefreshDisplaySubreddits();
		}

        private async void OnSettingsChanged(SettingsChangedMessage message)
        {
            if (!message.InitialLoad)
                await _baconProvider.GetService<ISettingsService>().Persist();
        }

        public virtual async void Activate()
        {
            MessengerInstance.Register<UserLoggedInMessage>(this, OnUserLoggedIn);
            MessengerInstance.Register<SelectSubredditMessage>(this, OnSubredditChanged);
            MessengerInstance.Register<SettingsChangedMessage>(this, OnSettingsChanged);
            if (_initialLoad)
            {
                OnUserLoggedIn(new UserLoggedInMessage { CurrentUser = await _userService.GetUser(), UserTriggered = false });
            }
        }

        public virtual void Deactivate()
        {
            MessengerInstance.Unregister<UserLoggedInMessage>(this);
            MessengerInstance.Unregister<SelectSubredditMessage>(this);
            MessengerInstance.Unregister<SettingsChangedMessage>(this);
        }

    }
}
