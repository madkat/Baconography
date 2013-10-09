using BaconographyPortable.Messages;
using BaconographyPortable.Model.Reddit;
using BaconographyPortable.Model.Reddit.ListingHelpers;
using BaconographyPortable.Services;
using BaconographyPortable.ViewModel.Collections;
using BaconographyW8;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Practices.ServiceLocation;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaconographyPortable.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        IBaconProvider _baconProvider;
        IRedditService _redditService;
        IDynamicViewLocator _dynamicViewLocator;
        INavigationService _navigationService;
        IUserService _userService;
        ILiveTileService _liveTileService;
        IOfflineService _offlineService;
        ISettingsService _settingsService;
		INotificationService _notificationService;

		bool _initialLoad = true;
		private ViewModelLocator _viewModelLocator;

		public MainViewModel(IBaconProvider baconProvider)
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
			_viewModelLocator = new ViewModelLocator();

			MessengerInstance.Register<UserLoggedInMessage>(this, OnUserLoggedIn);
			MessengerInstance.Register<SelectSubredditMessage>(this, OnSubredditChanged);
			MessengerInstance.Send<UserLoggedInMessage>(new UserLoggedInMessage { CurrentUser = _userService.GetUser().Result, UserTriggered = false });

			AboutSubredditVM = new AboutSubredditViewModel(_baconProvider, _viewModelLocator.Reddit.SelectedSubreddit, true);
        }

		private Subreddit _currentSubreddit;
		public Subreddit CurrentSubreddit
		{
			get
			{
				if (_viewModelLocator.Reddit.SelectedSubreddit != null)
					return _viewModelLocator.Reddit.SelectedSubreddit.Data;
				return null;
			}
		}

		private AboutSubredditViewModel _aboutSubredditVM;
		public AboutSubredditViewModel AboutSubredditVM
		{
			get
			{
				return _aboutSubredditVM;
			}
			private set
			{
				_aboutSubredditVM = value;
				RaisePropertyChanged("AboutSubredditVM");
			}
		}

		public bool HasSidebar
		{
			get
			{
				return _aboutSubredditVM != null;
			}
		}

		private void CheckSelections()
		{
			foreach (var sub in _displayedSubreddits)
			{
				if (CurrentSubreddit != null && sub.DisplayName == CurrentSubreddit.DisplayName)
					sub.Selected = true;
				else
				{
					if (sub.Selected)
						sub.Selected = false;
				}
			}
		}

		private async void OnSubredditChanged(SelectSubredditMessage message)
		{
			CheckSelections();
			var sublist = await _redditService.GetSubscribedSubreddits();
			AboutSubredditVM = new AboutSubredditViewModel(ServiceLocator.Current.GetInstance<IBaconProvider>(), message.Subreddit, sublist.Contains(message.Subreddit.Data.Name));
		}

		private async void OnUserLoggedIn(UserLoggedInMessage message)
		{
			LoggedIn = message.CurrentUser != null && !string.IsNullOrWhiteSpace(message.CurrentUser.LoginCookie);

			if (_initialLoad)
			{
				await LoadSubreddits();
				_initialLoad = false;
			}

			SubscribedSubreddits.Refresh();
		}

		bool _currentlySavingSubreddits = false;
		bool _suspendSaving = false;
		async void _subreddits_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
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

		public async Task SaveSubreddits()
		{
			try
			{
				await _offlineService.StoreOrderedThings("subredditlist", Subreddits);
			}
			catch { }

		}

		public async Task LoadSubreddits()
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

				RefreshDisplaySubreddits();
				RaisePropertyChanged("Subreddits");

				_subreddits.CollectionChanged += _subreddits_CollectionChanged;
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
    }
}
