﻿using GalaSoft.MvvmLight;
using SnooSharp;
using SnooStream.Common;
using SnooStream.Model;
using SnooStream.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SnooStream.ViewModel
{
    public class SnooStreamViewModel : ViewModelBase
    {
        public SnooStreamViewModel()
        {
            _listingFilter = new NSFWListingFilter();
            OfflineService = new OfflineService();
            RedditUserState = new UserState();
            NotificationService = new Common.NotificationService();
            RedditService = new Reddit(_listingFilter, RedditUserState, OfflineService, CaptchaProvider);
            _initializationBlob = OfflineService.LoadInitializationBlob("");
            Settings = new Model.Settings(_initializationBlob.Settings);
            _listingFilter.Initialize(Settings, OfflineService, RedditService, _initializationBlob.NSFWFilter);
            CommandDispatcher = new CommandDispatcher();
            UserHub = new UserHubViewModel(_initializationBlob);
            ModeratorHub = new ModeratorHubViewModel();
            SettingsHub = new SettingsViewModel();
            SubredditRiver = new SubredditRiverViewModel();
            if (_initializationBlob.LockscreenImages != null && _initializationBlob.LockscreenImages.Count > 0)
            {
                Random rnd = new Random();
                FeaturedImage = _initializationBlob.LockscreenImages[rnd.Next() % _initializationBlob.LockscreenImages.Count].Item2;
            }
            LoadLargeImages();
        }

        private async void LoadLargeImages()
        {
            await Task.Delay(2000); //stay away from startup, we've got enough going on as it is
            if (SystemServices.IsLowPriorityNetworkOk)
            {
                await NotificationService.Report("loading secondary images", async () =>
                {
                    var posts = await RedditService.GetPostsBySubreddit(Settings.LockScreenReddit, 25);
                    if (posts != null)
                    {
                        foreach (var post in posts.Data.Children)
                        {

                        }
                    }
                });
                
            }
            
        }

        private InitializationBlob _initializationBlob;
        private NSFWListingFilter _listingFilter;
        public static CommandDispatcher CommandDispatcher {get; set;}
        public static Settings Settings { get; set; }
        public static OfflineService OfflineService { get; private set; }
        public static UserState RedditUserState { get; private set; }
        public static Reddit RedditService { get; private set; }
        public static NotificationService NotificationService { get; private set; }
        public static ICaptchaProvider CaptchaProvider { get; set; }
        public static IMarkdownProcessor MarkdownProcessor { get; set; }
        public static IUserCredentialService UserCredentialService { get; set; }
        public static INavigationService NavigationService { get; set; }
        public static ISystemServices SystemServices { get; set; }

        public UserHubViewModel UserHub { get; private set; }
        public ModeratorHubViewModel ModeratorHub { get; private set; }
        public SettingsViewModel SettingsHub { get; private set; }
        public SubredditRiverViewModel SubredditRiver { get; private set; }

        public UploadViewModel UploadHub { get; private set; }
        public string FeaturedImage { get; private set; }

        public static CancellationToken UIContextCancellationToken { get; set; }
    }
}
