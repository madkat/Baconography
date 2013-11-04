using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using BaconographyWP8.View;
using BaconographyPortable.ViewModel;
using Microsoft.Practices.ServiceLocation;
using BaconographyPortable.Services;
using BaconographyWP8;
using BaconographyWP8Core.Common;
using BaconographyPortable.Messages;
using GalaSoft.MvvmLight.Messaging;
using BaconographyWP8.Messages;

namespace BaconographyWP8Core.View
{
    [ViewUri("/BaconographyWP8Core;component/View/SimpleRedditView.xaml")]
    public partial class SimpleRedditView : PhoneApplicationPage
    {
        ISettingsService _settingsService;
        IViewModelContextService _viewModelContextService;
        ISmartOfflineService _smartOfflineService;
        INavigationService _navigationService;
        IUserService _userService;

        public SimpleRedditView()
        {
            InitializeComponent();
            Loaded += RedditViewSingle_Loaded;
            Messenger.Default.Register<UserLoggedInMessage>(this, OnUserLoggedIn);
            _settingsService = ServiceLocator.Current.GetInstance<ISettingsService>();
            _viewModelContextService = ServiceLocator.Current.GetInstance<IViewModelContextService>();
            _smartOfflineService = ServiceLocator.Current.GetInstance<ISmartOfflineService>();
            _navigationService = ServiceLocator.Current.GetInstance<INavigationService>();
            _userService = ServiceLocator.Current.GetInstance<IUserService>();
            MaybeUserIsLoggedIn();
        }

        private async void MaybeUserIsLoggedIn()
        {
            try
            {
                var currentUser = await _userService.GetUser();
                if (currentUser != null && !string.IsNullOrWhiteSpace(currentUser.LoginCookie))
                    OnUserLoggedIn(new UserLoggedInMessage { CurrentUser = currentUser, UserTriggered = false });
            }
            catch { }
        }

        void RedditViewSingle_Loaded(object sender, RoutedEventArgs e)
        {
            CurrentRedditView = redditView;
        }



        public RedditView CurrentRedditView
        {
            get { return (RedditView)GetValue(CurrentRedditViewProperty); }
            set { SetValue(CurrentRedditViewProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentRedditViewProperty =
            DependencyProperty.Register("CurrentRedditViewProperty", typeof(RedditView), typeof(SimpleRedditView), new PropertyMetadata(null));


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
            Mail = 0,
            Settings,
            Sort
        }

        private string loginItemText = "login";

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

        private void BuildMenu()
        {
            var simpleLayoutMode = _settingsService.SimpleLayoutMode;

            appBarButtons = new List<ApplicationBarIconButton>();
            appMenuItems = new List<ApplicationBarMenuItem>();

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
                ApplicationBar.Buttons.Add(appBarButtons[(int)ButtonEnum.Mail] as IApplicationBarIconButton);
                ApplicationBar.Buttons.Add(appBarButtons[(int)ButtonEnum.Settings] as IApplicationBarIconButton);
                ApplicationBar.Buttons.Add(appBarButtons[(int)ButtonEnum.Sort] as IApplicationBarIconButton);
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
                if (ApplicationBar.MenuItems.Contains(appMenuItems[(int)MenuEnum.Sidebar]))
                    ApplicationBar.MenuItems.Remove(appMenuItems[(int)MenuEnum.Sidebar]);
            }
            else
            {
                if (!ApplicationBar.MenuItems.Contains(appMenuItems[(int)MenuEnum.Sidebar]))
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
    }
}