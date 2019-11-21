using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using TwitchClient.Helpers;
using TwitchClient.Services;
using Windows.Security.Authentication.Web;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using WinUI = Microsoft.UI.Xaml.Controls;

namespace TwitchClient.ViewModels
{
    public class ShellViewModel : ViewModelBase
    {
        private readonly KeyboardAccelerator altLeftKeyboardAccelerator = BuildKeyboardAccelerator(VirtualKey.Left, VirtualKeyModifiers.Menu);
        private readonly KeyboardAccelerator backKeyboardAccelerator = BuildKeyboardAccelerator(VirtualKey.GoBack);
        private ApplicationDataContainer localData;
        private string searchParam;
        private bool isBackEnabled;
        private IList<KeyboardAccelerator> keyboardAccelerators;
        private WinUI.NavigationView navigationView;
        private WinUI.NavigationViewItem selected;
        private ICommand loadedCommand;
        private ICommand itemInvokedCommand;
        private ICommand searchCommand;
        private ICommand authCommand;
        private ICommand themeCommand;

        public ShellViewModel()
        {
        }

        public static NavigationServiceEx NavigationService => ViewModelLocator.Current.NavigationService;

        public string SearchParam
        {
            get => searchParam;
            set
            {
                searchParam = value;
                RaisePropertyChanged("SearchParam");
            }
        }

        public bool IsBackEnabled
        {
            get => isBackEnabled;
            set => Set(ref isBackEnabled, value);
        }

        public WinUI.NavigationViewItem Selected
        {
            get => selected;
            set => Set(ref selected, value);
        }

        public ICommand LoadedCommand => loadedCommand ?? (loadedCommand = new RelayCommand(OnLoaded));

        public ICommand ItemInvokedCommand => itemInvokedCommand ?? (itemInvokedCommand = new RelayCommand<WinUI.NavigationViewItemInvokedEventArgs>(OnItemInvoked));

        public ICommand SearchCommand => searchCommand ?? (searchCommand = new RelayCommand(SearchStream));

        public ICommand AuthCommand => authCommand ?? (authCommand = new RelayCommand(Auth));

        public ICommand ThemeCommand => themeCommand ?? (themeCommand = new RelayCommand(async () => { await ThemeSelectorService.SwitchThemeAsync(); }));

        public void Initialize(Frame frame, WinUI.NavigationView navigationView, IList<KeyboardAccelerator> keyboardAccelerators)
        {
            this.navigationView = navigationView;
            this.keyboardAccelerators = keyboardAccelerators;
            NavigationService.Frame = frame;
            NavigationService.NavigationFailed += Frame_NavigationFailed;
            NavigationService.Navigated += Frame_Navigated;
            navigationView.BackRequested += OnBackRequested;
            localData = ApplicationData.Current.LocalSettings;
        }

        private static KeyboardAccelerator BuildKeyboardAccelerator(VirtualKey key, VirtualKeyModifiers? modifiers = null)
        {
            KeyboardAccelerator keyboardAccelerator = new KeyboardAccelerator() { Key = key };
            if (modifiers.HasValue)
            {
                keyboardAccelerator.Modifiers = modifiers.Value;
            }

            keyboardAccelerator.Invoked += OnKeyboardAcceleratorInvoked;
            return keyboardAccelerator;
        }

        private static void OnKeyboardAcceleratorInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            bool result = NavigationService.GoBack();
            args.Handled = result;
        }

        private async void Auth()
        {
            Match match;
            string oauth_token = string.Empty;
            Uri startUri = new Uri($"https://id.twitch.tv/oauth2/authorize?response_type=token&client_id=0pje11teayzq9z2najlxgdcc5d2dy1&redirect_uri=https://twitchapps.com/tokengen/&scope=chat%3Aread%20chat%3Aedit+user:read:email+user_read+channel_read+user_follows_edit");
            WebAuthenticationResult webAuthenticationResult = await WebAuthenticationBroker.AuthenticateAsync(WebAuthenticationOptions.None, startUri, new Uri("https://twitchapps.com/tokengen/"));
            Debug.WriteLine(WebAuthenticationBroker.GetCurrentApplicationCallbackUri());
            switch (webAuthenticationResult.ResponseStatus)
            {
                // Successful authentication.
                case WebAuthenticationStatus.Success:
                    Debug.WriteLine(webAuthenticationResult.ResponseData.ToString());
                    match = Regex.Match(webAuthenticationResult.ResponseData.ToString(), "#access_token=(?<token>.*)&scope=");
                    oauth_token = match.Groups["token"].Value;
                    localData.Values["OAuth"] = oauth_token;
                    break;

                // HTTP error.
                case WebAuthenticationStatus.ErrorHttp:
                    Debug.WriteLine(webAuthenticationResult.ResponseErrorDetail.ToString());
                    break;
                default:
                    Debug.WriteLine(webAuthenticationResult.ResponseData.ToString());
                    break;
            }
        }

        private void SearchStream()
        {
            localData.Values["Search_param"] = SearchParam;
            localData.Values["Search_type"] = "TitleBarSearch";
            NavigationService.Navigate("TwitchClient.ViewModels.SearchViewModel", SearchParam);
        }

        private async void OnLoaded()
        {
            keyboardAccelerators.Add(altLeftKeyboardAccelerator);
            keyboardAccelerators.Add(backKeyboardAccelerator);
            await Task.CompletedTask;
        }

        private void OnItemInvoked(WinUI.NavigationViewItemInvokedEventArgs args)
        {
            WinUI.NavigationViewItem item = navigationView.MenuItems
                            .OfType<WinUI.NavigationViewItem>()
                            .First(menuItem => (string)menuItem.Content == (string)args.InvokedItem);
            string pageKey = item.GetValue(NavHelper.NavigateToProperty) as string;
            NavigationService.Navigate(pageKey, new DrillInNavigationTransitionInfo());
        }

        private void OnBackRequested(WinUI.NavigationView sender, WinUI.NavigationViewBackRequestedEventArgs args)
        {
            NavigationService.GoBack();
        }

        private void Frame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw e.Exception;
        }

        private void Frame_Navigated(object sender, NavigationEventArgs e)
        {
            IsBackEnabled = NavigationService.CanGoBack;
            Selected = navigationView.MenuItems
                            .OfType<WinUI.NavigationViewItem>()
                            .FirstOrDefault(menuItem => IsMenuItemForPageType(menuItem, e.SourcePageType));
        }

        private bool IsMenuItemForPageType(WinUI.NavigationViewItem menuItem, Type sourcePageType)
        {
            string navigatedPageKey = NavigationService.GetNameOfRegisteredPage(sourcePageType);
            string pageKey = menuItem.GetValue(NavHelper.NavigateToProperty) as string;
            return pageKey == navigatedPageKey;
        }
    }
}
