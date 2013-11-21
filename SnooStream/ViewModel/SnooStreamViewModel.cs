using GalaSoft.MvvmLight;
using SnooSharp;
using SnooStream.Common;
using SnooStream.Model;
using SnooStream.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SnooStream.ViewModel
{
    public class SnooStreamViewModel : ViewModelBase
    {
        public SnooStreamViewModel(string currentWorkingDirectory)
        {
            _listingFilter = new NSFWListingFilter();
            OfflineService = new OfflineService(currentWorkingDirectory);
            RedditService = new Reddit(_listingFilter, null, OfflineService, CaptchaProvider);
            _initializationBlob = OfflineService.LoadInitializationBlob("");
            Settings = new Model.Settings(_initializationBlob.Settings);
            _listingFilter.Initialize(Settings, OfflineService, RedditService, _initializationBlob.NSFWFilter);
            CommandDispatcher = new CommandDispatcher();
        }

        private InitializationBlob _initializationBlob;
        private NSFWListingFilter _listingFilter;
        public static CommandDispatcher CommandDispatcher {get; set;}
        public static Settings Settings { get; set; }
        public static OfflineService OfflineService { get; private set; }
        public static Reddit RedditService { get; private set; }
        public static ICaptchaProvider CaptchaProvider { get; private set; }
        public static IMarkdownProcessor MarkdownProcessor { get; private set; }
        public static IUserCredentialService UserCredentialService { get; private set; }
        public static INotificationService NotificationService { get; private set; }
        public static INavigationService NavigationService { get; private set; }
        public static ISystemServices SystemServices { get; private set; }

        public UserHubViewModel UserHub { get; private set; }
        public ModeratorHubViewModel ModeratorHub { get; private set; }
        public SettingsViewModel SettingsHub { get; private set; }
        public LinkRiverViewModel RiverViewModel { get; private set; }

        public SubredditRiverViewModel MakeSubredditRiver(string subreddit)
        {
            return new SubredditRiverViewModel(subreddit, this);
        }

        public UploadViewModel UploadHub { get; private set; }

        public static CancellationToken UIContextCancellationToken { get; set; }
    }
}
