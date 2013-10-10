using BaconographyPortable.Messages;
using BaconographyPortable.Model.Reddit;
using BaconographyPortable.Services;
using BaconographyPortable.ViewModel.Collections;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaconographyPortable.ViewModel
{
    public class SubredditsViewModel : ViewModelBase
    {
        IRedditService _redditService;
        INavigationService _navigationService;
        IUserService _userService;
        IDynamicViewLocator _dynamicViewLocator;
        IBaconProvider _baconProvider;
		ISystemServices _systemServices;

        public SubredditsViewModel(IBaconProvider baconProvider)
        {
            _baconProvider = baconProvider;
            _redditService = _baconProvider.GetService<IRedditService>();
            _navigationService = _baconProvider.GetService<INavigationService>();
            _userService = _baconProvider.GetService<IUserService>();
            _dynamicViewLocator = _baconProvider.GetService<IDynamicViewLocator>();
			_systemServices = _baconProvider.GetService<ISystemServices>();

			Subreddits = new BindingShellViewModelCollection(new SubredditViewModelCollection(_baconProvider));
            //Subreddits = new SubredditViewModelCollection(_baconProvider);
			_gotoSubreddit = new RelayCommand(GotoSubredditImpl);
        }

		string _targetSubreddit;
		public string TargetSubreddit
		{
			get
			{
				return _targetSubreddit;
			}
			set
			{
				_targetSubreddit = value;
				RaisePropertyChanged("TargetSubreddit");
			}
		}

		RelayCommand _gotoSubreddit;
		public RelayCommand GotoSubreddit { get { return _gotoSubreddit; } }
		private async void GotoSubredditImpl()
		{
			if (string.IsNullOrWhiteSpace(TargetSubreddit))
			{
				_navigationService.Navigate(_dynamicViewLocator.RedditView, null);
			}
			else if (TargetSubreddit.StartsWith("/r/"))
			{
				TargetSubreddit = TargetSubreddit.Substring("/r/".Length);
			}

			_navigationService.Navigate(_dynamicViewLocator.RedditView, new SelectSubredditMessage { Subreddit = await _redditService.GetSubreddit(TargetSubreddit) });
		}

        public AboutSubredditViewModel SelectedSubreddit
        {
            get
            {
                return null;
            }
            set
            {
                _navigationService.GoBack();
                _navigationService.Navigate(_dynamicViewLocator.RedditView, new SelectSubredditMessage { Subreddit = value.Thing });
            }
        }

		private static string CleanRedditLink(string userInput, User user)
		{
			if (string.IsNullOrWhiteSpace(userInput))
				return "/";

			if (userInput == "/")
				return userInput;

			if (user != null && !string.IsNullOrWhiteSpace(user.Username))
			{
				var selfMulti = "/" + user.Username + "/m/";
				if (userInput.Contains(selfMulti))
				{
					return "/me/m/" + userInput.Substring(userInput.IndexOf(selfMulti) + selfMulti.Length);
				}
			}

			if (userInput.StartsWith("me/m/"))
				return "/" + userInput;
			else if (userInput.StartsWith("/m/"))
				return "/me" + userInput;
			else if (userInput.StartsWith("/me/m/"))
				return userInput;

			if (userInput.StartsWith("/u/"))
			{
				return userInput.Replace("/u/", "/user/");
			}

			if (userInput.StartsWith("r/"))
				return "/" + userInput;
			else if (userInput.StartsWith("/") && !userInput.StartsWith("/r/"))
				return "/r" + userInput;
			else if (userInput.StartsWith("/r/"))
				return userInput;
			else
				return "/r/" + userInput;
		}

		private string _text;
		public string Text
		{
			get
			{
				return _text;
			}
			set
			{
				bool wasChanged = _text != value;
				if (wasChanged)
				{
					_text = value;
					RaisePropertyChanged("Text");

					if (_text.Length < 3)
					{
						Subreddits.RevertToDefault();
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

		void queryTimer_Tick(object sender, object timer)
		{
			// Stop the timer so it doesn't fire again unless rescheduled
			RevokeQueryTimer();
			if (Subreddits != null)
			{
				if (!(_text != null && _text.Contains("/")))
					Subreddits.UpdateRealItems(new SearchResultsViewModelCollection(_baconProvider, _text, true));
			}
		}

		public BindingShellViewModelCollection Subreddits { get; set; }
    }
}
