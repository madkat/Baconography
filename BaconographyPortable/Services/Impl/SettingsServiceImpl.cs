using BaconographyPortable.Messages;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaconographyPortable.Services.Impl
{
    static class SettingsHelper
    {
        internal static string TryNullGet(this Dictionary<string, string> dict, string key)
            {
                string result;
                if (!dict.TryGetValue(key, out result))
                    return null;
                else
                    return result;
            }
    }
    public class SettingsServiceImpl : ISettingsService, IBaconService
    {
        IBaconProvider _baconProvider;
        bool _isOnline = true;
        public bool IsOnline()
        {
            return _isOnline;
        }

        public void SetOffline(bool fromUser)
        {
            _isOnline = false;
        }

        public void SetOnline(bool fromUser)
        {
            _isOnline = true;
        }

		public SettingsServiceImpl()
		{
			Messenger.Default.Register<ConnectionStatusMessage>(this, OnConnectionStatusChanged);
		}

		private void OnConnectionStatusChanged(ConnectionStatusMessage message)
		{
			if (message.IsOnline)
				SetOnline(message.UserInitiated);
			else
				SetOffline(message.UserInitiated);
		}

        public bool AllowOver18 { get; set; }
        public int MaxTopLevelOfflineComments { get; set; }
        public bool OfflineOnlyGetsFirstSet { get; set; }
        public bool OpenLinksInBrowser { get; set; }
        public bool HighlightAlreadyClickedLinks { get; set; }
        public bool ApplyReadabliltyToLinks { get; set; }
        public bool PreferImageLinksForTiles { get; set; }
        public int DefaultOfflineLinkCount { get; set; }
        public bool LeftHandedMode { get; set; }
        public bool OrientationLock { get; set; }
        public string Orientation { get; set; }
        public bool AllowPredictiveOfflining { get; set; }
        public bool PromptForCaptcha { get; set; }
        public bool AllowOver18Items { get; set; }
        public bool AllowPredictiveOffliningOnMeteredConnection { get; set; }
        public bool HighresLockScreenOnly { get; set; }
        public bool EnableUpdates { get; set; }
        public bool MessagesInLockScreenOverlay { get; set; }
        public bool PostsInLockScreenOverlay { get; set; }
        public int OverlayOpacity { get; set; }
        public string ImagesSubreddit { get; set; }
        public string LockScreenReddit { get; set; }
        public string LiveTileReddit { get; set; }
        public int ScreenWidth { get; set; }
        public int ScreenHeight { get; set; }
        public bool EnableOvernightUpdates { get; set; }
        public bool UpdateOverlayOnlyOnWifi { get; set; }
        public bool UpdateImagesOnlyOnWifi { get; set; }
        public bool TapForComments { get; set; }
        public bool UseImagePickerForLockScreen { get; set; }
        public bool RoundedLockScreen { get; set; }
        public bool MultiColorCommentMargins { get; set; }
        public bool OnlyFlipViewUnread { get; set; }
        public bool OnlyFlipViewImages { get; set; }
        public bool InvertSystemTheme { get; set; }

        public bool AllowAdvertising { get; set; }

        public int OverlayItemCount { get; set; }
        public int OfflineCacheDays { get; set; }
        public DateTime LastCleanedCache { get; set; }
        public DateTime LastUpdatedImages { get; set; }
        public bool SimpleLayoutMode { get; set; }
        public bool OneTouchVoteMode { get; set; }

        public bool DisableBackground { get; set; }

        public void ShowSettings()
        {

        }

        public async Task Initialize(IBaconProvider baconProvider)
        {
            _baconProvider = baconProvider;
            try
            {
                await LoadSettingsFromKDB();

                Messenger.Default.Send<SettingsChangedMessage>(new SettingsChangedMessage { InitialLoad = true });
            }
            catch
            {
                //not interested in failure here
            }
        }

        private async Task LoadSettingsFromKDB()
        {
            var offlineService = _baconProvider.GetService<IOfflineService>();

            var initBlob = await offlineService.LoadInitializationBlob(_baconProvider.GetService<IUserService>());


            var allowOver18String = initBlob.Settings.TryNullGet("AllowOver18");
            if (!string.IsNullOrWhiteSpace(allowOver18String))
                AllowOver18 = bool.Parse(allowOver18String);
            else
                AllowOver18 = false;

            var maxTopLevelOfflineCommentsString = initBlob.Settings.TryNullGet("MaxTopLevelOfflineComments");
            if (!string.IsNullOrWhiteSpace(maxTopLevelOfflineCommentsString))
                MaxTopLevelOfflineComments = int.Parse(maxTopLevelOfflineCommentsString);
            else
                MaxTopLevelOfflineComments = 50;

            var offlineOnlyGetsFirstSetString = initBlob.Settings.TryNullGet("OfflineOnlyGetsFirstSet");
            if (!string.IsNullOrWhiteSpace(offlineOnlyGetsFirstSetString))
                OfflineOnlyGetsFirstSet = bool.Parse(offlineOnlyGetsFirstSetString);
            else
                OfflineOnlyGetsFirstSet = true;

            var openLinksInBrowserString = initBlob.Settings.TryNullGet("OpenLinksInBrowser");
            if (!string.IsNullOrWhiteSpace(openLinksInBrowserString))
                OpenLinksInBrowser = bool.Parse(openLinksInBrowserString);
            else
                OpenLinksInBrowser = false;

            var highlightAlreadyClickedLinksString = initBlob.Settings.TryNullGet("HighlightAlreadyClickedLinks");
            if (!string.IsNullOrWhiteSpace(highlightAlreadyClickedLinksString))
                HighlightAlreadyClickedLinks = bool.Parse(highlightAlreadyClickedLinksString);
            else
                HighlightAlreadyClickedLinks = true;

            var applyReadabliltyToLinksString = initBlob.Settings.TryNullGet("ApplyReadabliltyToLinks");
            if (!string.IsNullOrWhiteSpace(applyReadabliltyToLinksString))
                ApplyReadabliltyToLinks = bool.Parse(applyReadabliltyToLinksString);
            else
                ApplyReadabliltyToLinks = false;

            var preferImageLinksForTiles = initBlob.Settings.TryNullGet("PreferImageLinksForTiles");
            if (!string.IsNullOrWhiteSpace(preferImageLinksForTiles))
                PreferImageLinksForTiles = bool.Parse(preferImageLinksForTiles);
            else
                PreferImageLinksForTiles = true;

            var defaultOfflineLinkCount = initBlob.Settings.TryNullGet("DefaultOfflineLinkCount");
            if (!string.IsNullOrWhiteSpace(defaultOfflineLinkCount))
                DefaultOfflineLinkCount = int.Parse(defaultOfflineLinkCount);
            else
                DefaultOfflineLinkCount = 25;

            var leftHandedMode = initBlob.Settings.TryNullGet("LeftHandedMode");
            if (!string.IsNullOrWhiteSpace(leftHandedMode))
                LeftHandedMode = bool.Parse(leftHandedMode);
            else
                LeftHandedMode = false;

            var orientationLock = initBlob.Settings.TryNullGet("OrientationLock");
            if (!string.IsNullOrWhiteSpace(orientationLock))
                OrientationLock = bool.Parse(orientationLock);
            else
                OrientationLock = false;

            var orientation = initBlob.Settings.TryNullGet("Orientation");
            if (!string.IsNullOrWhiteSpace(orientation))
                Orientation = orientation;
            else
                Orientation = "";

            var predicitveOfflining = initBlob.Settings.TryNullGet("AllowPredictiveOfflining");
            if (!string.IsNullOrWhiteSpace(predicitveOfflining))
                AllowPredictiveOfflining = bool.Parse(predicitveOfflining);
            else
                AllowPredictiveOfflining = false;

            PromptForCaptcha = true;
            var over18Items = initBlob.Settings.TryNullGet("AllowOver18Items");
            if (!string.IsNullOrWhiteSpace(over18Items))
                AllowOver18Items = bool.Parse(over18Items);
            else
                AllowOver18Items = false;

            var predictiveOffliningOnMeteredConnection = initBlob.Settings.TryNullGet("AllowPredictiveOffliningOnMeteredConnection");
            if (!string.IsNullOrWhiteSpace(predictiveOffliningOnMeteredConnection))
                AllowPredictiveOffliningOnMeteredConnection = bool.Parse(predictiveOffliningOnMeteredConnection);
            else
                AllowPredictiveOffliningOnMeteredConnection = false;

            var overlayOpacity = initBlob.Settings.TryNullGet("OverlayOpacity");
            if (!string.IsNullOrWhiteSpace(overlayOpacity))
                OverlayOpacity = int.Parse(overlayOpacity);
            else
                OverlayOpacity = 40;

            var highresLockScreenOnly = initBlob.Settings.TryNullGet("HighresLockScreenOnly");
            if (!string.IsNullOrWhiteSpace(highresLockScreenOnly))
                HighresLockScreenOnly = bool.Parse(highresLockScreenOnly);
            else
                HighresLockScreenOnly = false;

            var messagesInLockScreenOverlay = initBlob.Settings.TryNullGet("MessagesInLockScreenOverlay");
            if (!string.IsNullOrWhiteSpace(messagesInLockScreenOverlay))
                MessagesInLockScreenOverlay = bool.Parse(messagesInLockScreenOverlay);
            else
                MessagesInLockScreenOverlay = true;

            var enableUpdates = initBlob.Settings.TryNullGet("EnableUpdates");
            if (!string.IsNullOrWhiteSpace(enableUpdates))
                EnableUpdates = bool.Parse(enableUpdates);
            else
                EnableUpdates = true;

            var postsInLockScreenOverlay = initBlob.Settings.TryNullGet("PostsInLockScreenOverlay");
            if (!string.IsNullOrWhiteSpace(postsInLockScreenOverlay))
                PostsInLockScreenOverlay = bool.Parse(postsInLockScreenOverlay);
            else
                PostsInLockScreenOverlay = true;

            var imagesSubreddit = initBlob.Settings.TryNullGet("ImagesSubreddit");
            if (!string.IsNullOrWhiteSpace(imagesSubreddit))
                ImagesSubreddit = imagesSubreddit;
            else
                ImagesSubreddit = "/r/earthporn+InfrastructurePorn+MachinePorn";

            var lockScreenReddit = initBlob.Settings.TryNullGet("LockScreenReddit");
            if (!string.IsNullOrWhiteSpace(lockScreenReddit))
                LockScreenReddit = lockScreenReddit;
            else
                LockScreenReddit = "/";

            var liveTileReddit = initBlob.Settings.TryNullGet("LiveTileReddit");
            if (!string.IsNullOrWhiteSpace(liveTileReddit))
                LiveTileReddit = liveTileReddit;
            else
                LiveTileReddit = "/";

            var overlayItemCount = initBlob.Settings.TryNullGet("OverlayItemCount");
            if (!string.IsNullOrWhiteSpace(overlayItemCount))
                OverlayItemCount = int.Parse(overlayItemCount);
            else
                OverlayItemCount = 5;

            var offlineCacheDays = initBlob.Settings.TryNullGet("OfflineCacheDays");
            if (!string.IsNullOrWhiteSpace(offlineCacheDays))
                OfflineCacheDays = int.Parse(offlineCacheDays);
            else
                OfflineCacheDays = 2;

            var enableOvernightUpdates = initBlob.Settings.TryNullGet("EnableOvernightUpdates");
            if (!string.IsNullOrWhiteSpace(enableOvernightUpdates))
                EnableOvernightUpdates = bool.Parse(enableOvernightUpdates);
            else
                EnableOvernightUpdates = true;

            var updateOverlayOnlyOnWifi = initBlob.Settings.TryNullGet("UpdateOverlayOnlyOnWifi");
            if (!string.IsNullOrWhiteSpace(updateOverlayOnlyOnWifi))
                UpdateOverlayOnlyOnWifi = bool.Parse(updateOverlayOnlyOnWifi);
            else
                UpdateOverlayOnlyOnWifi = false;

            var updateImagesOnlyOnWifi = initBlob.Settings.TryNullGet("UpdateImagesOnlyOnWifi");
            if (!string.IsNullOrWhiteSpace(updateImagesOnlyOnWifi))
                UpdateImagesOnlyOnWifi = bool.Parse(updateImagesOnlyOnWifi);
            else
                UpdateImagesOnlyOnWifi = true;

            var allowAdvertising = initBlob.Settings.TryNullGet("AllowAdvertising");
            if (!string.IsNullOrWhiteSpace(allowAdvertising))
                AllowAdvertising = bool.Parse(allowAdvertising);
            else
                AllowAdvertising = true;

            var lastUpdatedImages = initBlob.Settings.TryNullGet("LastUpdatedImages");
            if (!string.IsNullOrWhiteSpace(lastUpdatedImages))
                LastUpdatedImages = DateTime.Parse(lastUpdatedImages);
            else
                LastUpdatedImages = new DateTime();

            var lastCleanedCache = initBlob.Settings.TryNullGet("LastCleanedCache");
            if (!string.IsNullOrWhiteSpace(lastCleanedCache))
                LastCleanedCache = DateTime.Parse(lastCleanedCache);
            else
                LastCleanedCache = new DateTime();

            var tapForComments = initBlob.Settings.TryNullGet("TapForComments");
            if (!string.IsNullOrWhiteSpace(tapForComments))
                TapForComments = bool.Parse(tapForComments);
            else
                TapForComments = false;

            var useImagePickerForLockScreen = initBlob.Settings.TryNullGet("UseImagePickerForLockScreen");
            if (!string.IsNullOrWhiteSpace(useImagePickerForLockScreen))
                UseImagePickerForLockScreen = bool.Parse(useImagePickerForLockScreen);
            else
                UseImagePickerForLockScreen = false;

            var roundedLockScreen = initBlob.Settings.TryNullGet("RoundedLockScreen");
            if (!string.IsNullOrWhiteSpace(roundedLockScreen))
                RoundedLockScreen = bool.Parse(roundedLockScreen);
            else
                RoundedLockScreen = false;

            var multiColoredCommentMargins = initBlob.Settings.TryNullGet("MultiColorCommentMargins");
            if (!string.IsNullOrWhiteSpace(multiColoredCommentMargins))
                MultiColorCommentMargins = bool.Parse(multiColoredCommentMargins);
            else
                MultiColorCommentMargins = false;

            var invertSystemTheme = initBlob.Settings.TryNullGet("InvertSystemTheme");
            if (!string.IsNullOrWhiteSpace(invertSystemTheme))
                InvertSystemTheme = bool.Parse(invertSystemTheme);
            else
                InvertSystemTheme = false;

            var onlyFlipViewUnread = initBlob.Settings.TryNullGet("OnlyFlipViewUnread");
            if (!string.IsNullOrWhiteSpace(onlyFlipViewUnread))
                OnlyFlipViewUnread = bool.Parse(onlyFlipViewUnread);
            else
                OnlyFlipViewUnread = false;

            var onlyFlipViewImages = initBlob.Settings.TryNullGet("OnlyFlipViewImages2");
            if (!string.IsNullOrWhiteSpace(onlyFlipViewImages))
                OnlyFlipViewImages = bool.Parse(onlyFlipViewImages);
            else
                OnlyFlipViewImages = true;

            var simpleLayoutMode = initBlob.Settings.TryNullGet("SimpleLayoutMode");
            if (!string.IsNullOrWhiteSpace(simpleLayoutMode))
                SimpleLayoutMode = bool.Parse(simpleLayoutMode);
            else
                SimpleLayoutMode = false;

            var oneTouchVoteMode = initBlob.Settings.TryNullGet("OneTouchVoteMode2");
            if (!string.IsNullOrWhiteSpace(oneTouchVoteMode))
                OneTouchVoteMode = bool.Parse(oneTouchVoteMode);
            else
                OneTouchVoteMode = true;

            var disableBackground = initBlob.Settings.TryNullGet("DisableBackground");
            if (!string.IsNullOrWhiteSpace(disableBackground))
                DisableBackground = bool.Parse(disableBackground);
            else
                DisableBackground = false;
        }

        public async Task Persist()
        {
            var offlineService = _baconProvider.GetService<IOfflineService>();

            await offlineService.StoreInitializationBlob((initBlob) => 
            {
                initBlob.Settings["AllowOver18"] = AllowOver18.ToString();
                initBlob.Settings["MaxTopLevelOfflineComments"] = MaxTopLevelOfflineComments.ToString();
                initBlob.Settings["OfflineOnlyGetsFirstSet"] = OfflineOnlyGetsFirstSet.ToString();
                initBlob.Settings["OpenLinksInBrowser"] = OpenLinksInBrowser.ToString();
                initBlob.Settings["HighlightAlreadyClickedLinks"] = HighlightAlreadyClickedLinks.ToString();
                initBlob.Settings["ApplyReadabliltyToLinks"] = ApplyReadabliltyToLinks.ToString();
                initBlob.Settings["PreferImageLinksForTiles"] = PreferImageLinksForTiles.ToString();
                initBlob.Settings["DefaultOfflineLinkCount"] = DefaultOfflineLinkCount.ToString();
                initBlob.Settings["LeftHandedMode"] = LeftHandedMode.ToString();
                initBlob.Settings["OrientationLock"] = OrientationLock.ToString();
                initBlob.Settings["Orientation"] = Orientation.ToString();
                initBlob.Settings["AllowPredictiveOfflining"] = AllowPredictiveOfflining.ToString();
                initBlob.Settings["AllowPredictiveOffliningOnMeteredConnection"] = AllowPredictiveOffliningOnMeteredConnection.ToString();
                initBlob.Settings["AllowOver18Items"] = AllowOver18Items.ToString();
                initBlob.Settings["OverlayOpacity"] = OverlayOpacity.ToString();
                initBlob.Settings["HighresLockScreenOnly"] = HighresLockScreenOnly.ToString();
                initBlob.Settings["OverlayItemCount"] = OverlayItemCount.ToString();
                initBlob.Settings["OfflineCacheDays"] = OfflineCacheDays.ToString();
                initBlob.Settings["MessagesInLockScreenOverlay"] = MessagesInLockScreenOverlay.ToString();
                initBlob.Settings["EnableUpdates"] = EnableUpdates.ToString();
                initBlob.Settings["PostsInLockScreenOverlay"] = PostsInLockScreenOverlay.ToString();
                initBlob.Settings["ImagesSubreddit"] = ImagesSubreddit.ToString();
                initBlob.Settings["LockScreenReddit"] = LockScreenReddit.ToString();
                initBlob.Settings["LiveTileReddit"] = LiveTileReddit.ToString();
                initBlob.Settings["AllowAdvertising"] = AllowAdvertising.ToString();
                initBlob.Settings["EnableOvernightUpdates"] = EnableOvernightUpdates.ToString();
                initBlob.Settings["UpdateOverlayOnlyOnWifi"] = UpdateOverlayOnlyOnWifi.ToString();
                initBlob.Settings["UpdateImagesOnlyOnWifi"] = UpdateImagesOnlyOnWifi.ToString();
                initBlob.Settings["LastUpdatedImages"] = LastUpdatedImages.ToString();
                initBlob.Settings["LastCleanedCache"] = LastCleanedCache.ToString();
                initBlob.Settings["TapForComments"] = TapForComments.ToString();
                initBlob.Settings["RoundedLockScreen"] = RoundedLockScreen.ToString();
                initBlob.Settings["UseImagePickerForLockScreen"] = UseImagePickerForLockScreen.ToString();
                initBlob.Settings["MultiColorCommentMargins"] = MultiColorCommentMargins.ToString();
                initBlob.Settings["InvertSystemTheme"] = InvertSystemTheme.ToString();
                initBlob.Settings["OnlyFlipViewUnread"] = OnlyFlipViewUnread.ToString();
                initBlob.Settings["OnlyFlipViewImages2"] = OnlyFlipViewImages.ToString();
                initBlob.Settings["SimpleLayoutMode"] = SimpleLayoutMode.ToString();
                initBlob.Settings["DisableBackground"] = DisableBackground.ToString();
                initBlob.Settings["OneTouchVoteMode2"] = OneTouchVoteMode.ToString();

            });
        }


        public async Task ClearHistory()
        {
            var offlineService = _baconProvider.GetService<IOfflineService>();
            await offlineService.ClearHistory();
        }
    }
}
