using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using BaconographyPortable.Services;
using Microsoft.Practices.ServiceLocation;
using BaconographyPortable.ViewModel;
using Newtonsoft.Json;
using BaconographyWP8.View;
using GalaSoft.MvvmLight.Messaging;
using BaconographyPortable.Messages;
using BaconographyWP8.Messages;
using BaconographyWP8Core;
using BaconographyWP8.ViewModel;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using Microsoft.Phone.Reactive;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight;
using BaconographyWP8Core.Common;
using BaconographyWP8.Converters;
using BaconographyWP8Core.View;
using ReviewNotifier.Apollo;
using System.Windows.Data;
using System.Collections.Specialized;

namespace BaconographyWP8
{
    [ViewUri("/BaconographyWP8Core;component/MainPage.xaml")]
    public partial class MainPage : PhoneApplicationPage
    {

        // Constructor
		ISettingsService _settingsService;
        IViewModelContextService _viewModelContextService;
        ISmartOfflineService _smartOfflineService;
        INavigationService _navigationService;
        IUserService _userService;
        public MainPage()
        {
            InitializeComponent();
            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
            Loaded +=MainPage_Loaded;
			Messenger.Default.Register<UserLoggedInMessage>(this, OnUserLoggedIn);
			_settingsService = ServiceLocator.Current.GetInstance<ISettingsService>();
            _viewModelContextService = ServiceLocator.Current.GetInstance<IViewModelContextService>();
            _smartOfflineService = ServiceLocator.Current.GetInstance<ISmartOfflineService>();
            _navigationService = ServiceLocator.Current.GetInstance<INavigationService>();
            _userService = ServiceLocator.Current.GetInstance<IUserService>();
            MaybeUserIsLoggedIn();
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            ReviewNotification.TriggerAsync("Like what we're doing? Please leave us a review on the store\n\nGot a Nag? Let us know what we can improve on /r/baconography", "Review", 5);
        }

        public RedditView CurrentRedditView
        {
            get { return (RedditView)GetValue(CurrentRedditViewProperty); }
            set { SetValue(CurrentRedditViewProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CurrentRedditView.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentRedditViewProperty =
            DependencyProperty.Register("CurrentRedditView", typeof(RedditView), typeof(MainPage), new PropertyMetadata(null));



		private void AdjustForOrientation(PageOrientation orientation)
		{
			Messenger.Default.Send<OrientationChangedMessage>(new OrientationChangedMessage { Orientation = orientation });
			lastKnownOrientation = orientation;

			if (LayoutRoot != null)
			{
				if (orientation == PageOrientation.LandscapeRight)
					LayoutRoot.Margin = new Thickness(40, 0, 0, 0);
				else if (orientation == PageOrientation.LandscapeLeft)
					LayoutRoot.Margin = new Thickness(0, 0, 35, 0);
				else
					LayoutRoot.Margin = new Thickness(0, 0, 0, 0);
			}
		}

		PageOrientation lastKnownOrientation;


        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back && DataContext is ViewModelBase)
            {
                _viewModelContextService.PopViewModelContext(DataContext as ViewModelBase);
            }
            base.OnNavigatingFrom(e);
        }

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{

			this.AdjustForOrientation(this.Orientation);

			if (e.NavigationMode == NavigationMode.Back)
			{
                if (CurrentRedditView != null)
                {
                    CurrentRedditView.LoadWithScroll();
                }
			}
            else if (e.NavigationMode == NavigationMode.Refresh)
            {

            }
            else if (e.NavigationMode == NavigationMode.New)
            {
                if(DataContext is ViewModelBase)
                    _viewModelContextService.PushViewModelContext(DataContext as ViewModelBase);

                _smartOfflineService.NavigatedToView(typeof(MainPage), (e.NavigationMode == NavigationMode.New));

                if (this.NavigationContext.QueryString.ContainsKey("data") && !string.IsNullOrWhiteSpace(this.NavigationContext.QueryString["data"]))
                {
                    try
                    {
                        var unescapedData = Uri.UnescapeDataString(this.NavigationContext.QueryString["data"]);
                        var deserializedObject = JsonConvert.DeserializeObject<SelectTemporaryRedditMessage>(unescapedData);
                        if (deserializedObject is SelectTemporaryRedditMessage)
                        {
                            Messenger.Default.Send<SelectTemporaryRedditMessage>(deserializedObject as SelectTemporaryRedditMessage);
                            int indexToPosition;
                            if (DataContext != null && (((MultipleRedditMainViewModel)DataContext).FindSubredditMessageIndex(deserializedObject as SelectTemporaryRedditMessage, out indexToPosition)))
                            {
                                Messenger.Default.Send<SelectIndexMessage>(new SelectIndexMessage { Index = indexToPosition, TypeContext = typeof(MultipleRedditMainViewModel)});
                            }
                        }
                    }
                    catch (UriFormatException)
                    {
                        ServiceLocator.Current.GetInstance<IBaconProvider>().GetService<INotificationService>().CreateNotification("Invalid main page uri state, please PM /u/hippiehunter with details");
                    }
                }
                else if (this.NavigationContext.QueryString.ContainsKey("WallpaperSettings"))
                {
                    _navigationService.Navigate<SettingsPageView>("lockscreen");
                    while (NavigationService.BackStack.Count() > 0)
                        NavigationService.RemoveBackEntry();
                    return;
                }
            }

            
		}

		protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
		{
			if (sortPopup.IsOpen == true)
			{
				sortPopup.IsOpen = false;
				e.Cancel = true;
			}
			else
			{
				base.OnBackKeyPress(e);
			}
		}

		protected override void OnOrientationChanged(OrientationChangedEventArgs e)
		{
			AdjustForOrientation(e.Orientation);
			base.OnOrientationChanged(e);
		}

		private string loginItemText = "login";

        private async void MaybeUserIsLoggedIn()
        {
            try
            {
                var currentUser = await _userService.GetUser();
                if (currentUser != null && !string.IsNullOrWhiteSpace(currentUser.LoginCookie))
                    OnUserLoggedIn(new UserLoggedInMessage { CurrentUser = currentUser, UserTriggered = false });
            }
            catch {}
        }

		private void OnUserLoggedIn(UserLoggedInMessage message)
		{
            if (appMenuItems == null || ApplicationBar.MenuItems.Count == 0)
                BuildMenu();

			bool loggedIn = message.CurrentUser != null && message.CurrentUser.Username != null;

			if (loggedIn)
			{
				loginItemText = "switch user / logout";
			}
			else
			{
				loginItemText = "login";
			}

            appMenuItems[(int)MenuEnum.Login].Text = loginItemText;

            if (loggedIn)
            {
                appBarButtons[(int)ButtonEnum.Mail].IsEnabled = true;
                appMenuItems[(int)MenuEnum.Submit].IsEnabled = true;
            }
            else
            {
                appBarButtons[(int)ButtonEnum.Mail].IsEnabled = false;
                appMenuItems[(int)MenuEnum.Submit].IsEnabled = false;
            }
		}

		private void MenuLogin_Click(object sender, EventArgs e)
		{
			var _navigationService = ServiceLocator.Current.GetInstance<INavigationService>();
			_navigationService.Navigate(typeof(LoginPageView), null);
		}

		private void MenuClose_Click(object sender, EventArgs e)
		{
            if (CurrentRedditView != null && CurrentRedditView.DataContext is RedditViewModel)
			{
				Messenger.Default.Send<CloseSubredditMessage>(new CloseSubredditMessage { Heading = ((RedditViewModel)CurrentRedditView.DataContext).Heading });
			}
		}

		private void MenuPin_Click(object sender, EventArgs e)
		{
            if (CurrentRedditView == null)
                return;

            var trvm = CurrentRedditView.DataContext as RedditViewModel;
			if (trvm == null || !trvm.IsTemporary)
				return;

			Messenger.Default.Send<CloseSubredditMessage>(new CloseSubredditMessage { Heading = trvm.Heading });
			Messenger.Default.Send<SelectSubredditMessage>(new SelectSubredditMessage { Subreddit = trvm.SelectedSubreddit, AddOnly = false });
		}

		private void MenuSettings_Click(object sender, EventArgs e)
		{
			var _navigationService = ServiceLocator.Current.GetInstance<INavigationService>();
			_navigationService.Navigate(typeof(SettingsPageView), null);
		}

        private void MenuMail_Click(object sender, EventArgs e)
        {
            var _navigationService = ServiceLocator.Current.GetInstance<INavigationService>();
            _navigationService.Navigate(typeof(MessagingPageView), null);
        }

        private void MenuSubmit_Click(object sender, EventArgs e)
        {
            var locator = Styles.Resources["Locator"] as ViewModelLocator;
            if (locator != null)
            {
                locator.Submit.RefreshUser.Execute(locator.Submit);
            }
            var _navigationService = ServiceLocator.Current.GetInstance<INavigationService>();
            _navigationService.Navigate(typeof(ComposePostPageView), null);
        }

		private void MenuManage_Click(object sender, EventArgs e)
		{
			var _navigationService = ServiceLocator.Current.GetInstance<INavigationService>();
			_navigationService.Navigate(typeof(SimpleSubredditManagerView), null);
		}

		private void MenuSort_Click(object sender, EventArgs e)
		{
			double height = 480;
			double width = 325;

			if (LayoutRoot.ActualHeight <= 480)
				height = LayoutRoot.ActualHeight;
			
			sortPopup.Height = height;
			sortPopup.Width = width;

            RedditViewModel rvm = CurrentRedditView.DataContext as RedditViewModel;
			if (rvm == null)
			    return;
			

			var child = sortPopup.Child as SelectSortTypeView;
			if (child == null)
				child = new SelectSortTypeView();
			child.SortOrder = rvm.SortOrder;
			child.Height = height;
			child.Width = width;
			child.button_ok.Click += (object buttonSender, RoutedEventArgs buttonArgs) =>
			{
				sortPopup.IsOpen = false;
				rvm.SortOrder = child.SortOrder;
			};

			child.button_cancel.Click += (object buttonSender, RoutedEventArgs buttonArgs) =>
			{
				sortPopup.IsOpen = false;
			};

			sortPopup.Child = child;
			sortPopup.IsOpen = true;
		}

		List<ApplicationBarMenuItem> appMenuItems;
        List<ApplicationBarIconButton> appBarButtons;

		enum MenuEnum
		{
			Login = 0,
            Search,
            Sidebar,
            Submit,
			Close,
			Pin
		}

        enum ButtonEnum
        {
            ManageSubreddits = 0,
            Mail,
            Settings,
            Sort
        }

		private void BuildMenu()
		{
            var simpleLayoutMode = _settingsService.SimpleLayoutMode;

            appBarButtons = new List<ApplicationBarIconButton>();
			appMenuItems = new List<ApplicationBarMenuItem>();

            appBarButtons.Add(new ApplicationBarIconButton());
            appBarButtons[(int)ButtonEnum.ManageSubreddits].IconUri = new Uri("\\Assets\\Icons\\manage.png", UriKind.Relative);
            appBarButtons[(int)ButtonEnum.ManageSubreddits].Text = "manage subs";
            appBarButtons[(int)ButtonEnum.ManageSubreddits].IsEnabled = true;
            appBarButtons[(int)ButtonEnum.ManageSubreddits].Click += MenuManage_Click;

            appBarButtons.Add(new ApplicationBarIconButton());
            SetMailButtonIcon(null);
            appBarButtons[(int)ButtonEnum.Mail].Text = "mail";
            appBarButtons[(int)ButtonEnum.Mail].IsEnabled = false;
            appBarButtons[(int)ButtonEnum.Mail].Click += MenuMail_Click;

            ServiceLocator.Current.GetInstance<MessagesViewModel>().PropertyChanged += (sender, args) => 
            {
                SetMailButtonIcon(args);
            };


            appBarButtons.Add(new ApplicationBarIconButton());
            appBarButtons[(int)ButtonEnum.Settings].IconUri = new Uri("\\Assets\\Icons\\settings.png", UriKind.Relative);
            appBarButtons[(int)ButtonEnum.Settings].Text = "settings";
            appBarButtons[(int)ButtonEnum.Settings].IsEnabled = true;
            appBarButtons[(int)ButtonEnum.Settings].Click += MenuSettings_Click;

            appBarButtons.Add(new ApplicationBarIconButton());
            appBarButtons[(int)ButtonEnum.Sort].IconUri = new Uri("\\Assets\\Icons\\sort.png", UriKind.Relative);
            appBarButtons[(int)ButtonEnum.Sort].Text = "sort";
            appBarButtons[(int)ButtonEnum.Sort].IsEnabled = true;
            appBarButtons[(int)ButtonEnum.Sort].Click += MenuSort_Click;

            ApplicationBar.Buttons.Clear();
            try
            {
                if (simpleLayoutMode)
                {
                    ApplicationBar.Buttons.Add(appBarButtons[(int)ButtonEnum.Mail] as IApplicationBarIconButton);
                    ApplicationBar.Buttons.Add(appBarButtons[(int)ButtonEnum.Settings] as IApplicationBarIconButton);
                }
                else
                {
                    foreach (var button in appBarButtons)
                        ApplicationBar.Buttons.Add(button as IApplicationBarIconButton);
                }
            }
            catch (Exception e)
            {

            }

			appMenuItems.Add(new ApplicationBarMenuItem());
			appMenuItems[(int)MenuEnum.Login].Text = loginItemText;
			appMenuItems[(int)MenuEnum.Login].IsEnabled = true;
			appMenuItems[(int)MenuEnum.Login].Click += MenuLogin_Click;

            appMenuItems.Add(new ApplicationBarMenuItem());
            appMenuItems[(int)MenuEnum.Search].Text = "search";
            appMenuItems[(int)MenuEnum.Search].IsEnabled = true;
            appMenuItems[(int)MenuEnum.Search].Click += MenuSearch_Click;

            appMenuItems.Add(new ApplicationBarMenuItem());
            appMenuItems[(int)MenuEnum.Sidebar].Text = "sidebar";
            appMenuItems[(int)MenuEnum.Sidebar].IsEnabled = true;
            appMenuItems[(int)MenuEnum.Sidebar].Click += MenuSidebar_Click;

            appMenuItems.Add(new ApplicationBarMenuItem());
            appMenuItems[(int)MenuEnum.Submit].Text = "new post";
            appMenuItems[(int)MenuEnum.Submit].IsEnabled = false;
            appMenuItems[(int)MenuEnum.Submit].Click += MenuSubmit_Click;

			appMenuItems.Add(new ApplicationBarMenuItem());
			appMenuItems[(int)MenuEnum.Close].Text = "close subreddit";
			appMenuItems[(int)MenuEnum.Close].IsEnabled = true;
			appMenuItems[(int)MenuEnum.Close].Click += MenuClose_Click;

			appMenuItems.Add(new ApplicationBarMenuItem());
			appMenuItems[(int)MenuEnum.Pin].Text = "pin subreddit";
			appMenuItems[(int)MenuEnum.Pin].IsEnabled = true;
			appMenuItems[(int)MenuEnum.Pin].Click += MenuPin_Click;         

			ApplicationBar.MenuItems.Clear();
            ApplicationBar.MenuItems.Add(appMenuItems[(int)MenuEnum.Login]);
            ApplicationBar.MenuItems.Add(appMenuItems[(int)MenuEnum.Search]);
            ApplicationBar.MenuItems.Add(appMenuItems[(int)MenuEnum.Submit]);
		}

        private void MenuSidebar_Click(object sender, EventArgs e)
        {
            if (CurrentRedditView != null && CurrentRedditView.DataContext is RedditViewModel)
            {
                var vm = CurrentRedditView.DataContext as RedditViewModel;

                var _navigationService = ServiceLocator.Current.GetInstance<INavigationService>();
                _navigationService.Navigate(typeof(AboutSubreddit), new Tuple<string>(vm.SelectedSubreddit.Data.Url));
            }
        }

        private void SetMailButtonIcon(System.ComponentModel.PropertyChangedEventArgs args)
        {
            if (args == null || args.PropertyName == "HasMail")
            {
                if (ServiceLocator.Current.GetInstance<MessagesViewModel>().HasMail)
                    appBarButtons[(int)ButtonEnum.Mail].IconUri = new Uri("\\Assets\\Icons\\read.png", UriKind.Relative);
                else
                    appBarButtons[(int)ButtonEnum.Mail].IconUri = new Uri("\\Assets\\Icons\\email.png", UriKind.Relative);
            }
        }

        private void MenuSearch_Click(object sender, EventArgs e)
        {
            var _navigationService = ServiceLocator.Current.GetInstance<INavigationService>();
            _navigationService.Navigate(typeof(SearchView), null);
        }

		private void UpdateMenuItems()
		{
			if (appMenuItems == null || ApplicationBar.MenuItems.Count == 0)
				BuildMenu();

            var selectedViewModel = CurrentRedditView == null ? null : CurrentRedditView.DataContext as RedditViewModel;

            if (selectedViewModel != null && selectedViewModel.IsMultiReddit)
            {
                if(ApplicationBar.MenuItems.Contains(appMenuItems[(int)MenuEnum.Sidebar]))
                    ApplicationBar.MenuItems.Remove(appMenuItems[(int)MenuEnum.Sidebar]);
            }
            else
            {
                if(!ApplicationBar.MenuItems.Contains(appMenuItems[(int)MenuEnum.Sidebar]))
                    ApplicationBar.MenuItems.Insert(0, appMenuItems[(int)MenuEnum.Sidebar]);
            }

            if (selectedViewModel != null && selectedViewModel.IsTemporary)
            {
                if (!ApplicationBar.MenuItems.Contains(appMenuItems[(int)MenuEnum.Close]))
                {
                    ApplicationBar.MenuItems.Insert(0, appMenuItems[(int)MenuEnum.Close]);
                    ApplicationBar.MenuItems.Insert(0, appMenuItems[(int)MenuEnum.Pin]);
                }
            }
            else if (ApplicationBar.MenuItems.Contains(appMenuItems[(int)MenuEnum.Close]))
            {
                ApplicationBar.MenuItems.Remove(appMenuItems[(int)MenuEnum.Close]);
                ApplicationBar.MenuItems.Remove(appMenuItems[(int)MenuEnum.Pin]);
            }
		}

		private void OnLoadedPivotItem(object sender, PivotItemEventArgs e)
		{
			UpdateMenuItems();
		}

        int appBarState = 0;
        private void appBar_StateChanged(object sender, ApplicationBarStateChangedEventArgs e)
        {
            //switch (appBarState++)
            //{
            //    case 0:
            //        ApplicationBar.IsMenuEnabled = false;
            //        ApplicationBar.Mode = ApplicationBarMode.Default;
            //        ApplicationBar.IsMenuEnabled = true;
            //        break;
            //    case 2:
            //        ApplicationBar.Mode = ApplicationBarMode.Minimized;
            //        break;
            //    case 4:
            //        ApplicationBar.IsMenuEnabled = false;
            //        ApplicationBar.Mode = ApplicationBarMode.Default;
            //        ApplicationBar.Mode = ApplicationBarMode.Minimized;
            //        ApplicationBar.IsMenuEnabled = true;
            //        appBarState = 0;
            //        break;
            //}
        }

        private void pivot_DoubleTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (!(e.OriginalSource is TextBlock))
                return;

            if (CurrentRedditView != null)
            {
                var context = CurrentRedditView.DataContext as RedditViewModel;
                var source = e.OriginalSource as TextBlock;
                if (source.Text != context.Heading)
                    return;

                CurrentRedditView.linksView.ScrollTo(context.Links.First());   
            }
        }

        // Sample code for building a localized ApplicationBar
        //private void BuildLocalizedApplicationBar()
        //{
        //    // Set the page's ApplicationBar to a new instance of ApplicationBar.
        //    ApplicationBar = new ApplicationBar();

        //    // Create a new button and set the text value to the localized string from AppResources.
        //    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
        //    appBarButton.Text = AppResources.AppBarButtonText;
        //    ApplicationBar.Buttons.Add(appBarButton);

        //    // Create a new menu item with the localized string from AppResources.
        //    ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
        //    ApplicationBar.MenuItems.Add(appBarMenuItem);
        //}
    }
}