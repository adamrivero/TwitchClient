using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using TwitchClient.Helpers;
using TwitchClient.Services;
using TwitchClient.Views;
using Windows.Security.Authentication.Web;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

using WinUI = Microsoft.UI.Xaml.Controls;

namespace TwitchClient.ViewModels
{
    public class ShellViewModel : ViewModelBase
    {
        StartServer ServerStart;
        private readonly KeyboardAccelerator _altLeftKeyboardAccelerator = BuildKeyboardAccelerator(VirtualKey.Left, VirtualKeyModifiers.Menu);
        private readonly KeyboardAccelerator _backKeyboardAccelerator = BuildKeyboardAccelerator(VirtualKey.GoBack);
        ApplicationDataContainer localData;
        private string searchParam;
        public string SearchParam
        {
            get { return searchParam; }
            set
            {
                searchParam = value;
                RaisePropertyChanged("SearchParam");
            }
        }
        private bool _isBackEnabled;
        private IList<KeyboardAccelerator> _keyboardAccelerators;
        private WinUI.NavigationView _navigationView;
        private WinUI.NavigationViewItem _selected;
        private ICommand _loadedCommand;
        private ICommand _itemInvokedCommand;
        private ICommand _searchCommand;
        private ICommand _authCommand;

        public bool IsBackEnabled
        {
            get { return _isBackEnabled; }
            set { Set(ref _isBackEnabled, value); }
        }

        public static NavigationServiceEx NavigationService => ViewModelLocator.Current.NavigationService;

        public WinUI.NavigationViewItem Selected
        {
            get { return _selected; }
            set { Set(ref _selected, value); }
        }

        public ICommand LoadedCommand => _loadedCommand ?? (_loadedCommand = new RelayCommand(OnLoaded));
        public ICommand ItemInvokedCommand => _itemInvokedCommand ?? (_itemInvokedCommand = new RelayCommand<WinUI.NavigationViewItemInvokedEventArgs>(OnItemInvoked));
        public ICommand SearchCommand => _searchCommand ?? (_searchCommand = new RelayCommand(SearchStream));
        public ICommand AuthCommand => _authCommand ?? (_authCommand = new RelayCommand(Auth));

        private async void Auth()
        {
            Match match;
            string oauth_token = "";
            Uri StartUri = new Uri($"https://id.twitch.tv/oauth2/authorize?response_type=token&client_id=0pje11teayzq9z2najlxgdcc5d2dy1&redirect_uri=https://twitchapps.com/tokengen/");
            WebAuthenticationResult webAuthenticationResult = await WebAuthenticationBroker.AuthenticateAsync(WebAuthenticationOptions.None, StartUri, new Uri("https://twitchapps.com/tokengen/"));
            Debug.WriteLine(WebAuthenticationBroker.GetCurrentApplicationCallbackUri());
            switch (webAuthenticationResult.ResponseStatus)
            {
                // Successful authentication.  
                case WebAuthenticationStatus.Success:
                    Debug.WriteLine(webAuthenticationResult.ResponseData.ToString());
                    match = Regex.Match(webAuthenticationResult.ResponseData.ToString(), "#access_token=(?<token>.*)&scope=");
                    oauth_token = match.Groups["token"].Value;
                    Debug.WriteLine(oauth_token);
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
            Debug.WriteLine("SAdasjhdkjsahdjas");
        }
        public ShellViewModel()
        {
        }

        public void Initialize(Frame frame, WinUI.NavigationView navigationView, IList<KeyboardAccelerator> keyboardAccelerators)
        {
            _navigationView = navigationView;
            _keyboardAccelerators = keyboardAccelerators;
            NavigationService.Frame = frame;
            NavigationService.NavigationFailed += Frame_NavigationFailed;
            NavigationService.Navigated += Frame_Navigated;
            _navigationView.BackRequested += OnBackRequested;
            localData = ApplicationData.Current.LocalSettings;
        }

        private async void OnLoaded()
        {
            // Keyboard accelerators are added here to avoid showing 'Alt + left' tooltip on the page.
            // More info on tracking issue https://github.com/Microsoft/microsoft-ui-xaml/issues/8
            ServerStart = new StartServer();
            await Task.Run(() => ServerStart.DoWork());
            _keyboardAccelerators.Add(_altLeftKeyboardAccelerator);
            _keyboardAccelerators.Add(_backKeyboardAccelerator);
            await Task.CompletedTask;
        }

        private void OnItemInvoked(WinUI.NavigationViewItemInvokedEventArgs args)
        {
            var item = _navigationView.MenuItems
                            .OfType<WinUI.NavigationViewItem>()
                            .First(menuItem => (string)menuItem.Content == (string)args.InvokedItem);
            var pageKey = item.GetValue(NavHelper.NavigateToProperty) as string;
            NavigationService.Navigate(pageKey, null, new DrillInNavigationTransitionInfo());
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
            Selected = _navigationView.MenuItems
                            .OfType<WinUI.NavigationViewItem>()
                            .FirstOrDefault(menuItem => IsMenuItemForPageType(menuItem, e.SourcePageType));
        }

        private bool IsMenuItemForPageType(WinUI.NavigationViewItem menuItem, Type sourcePageType)
        {
            var navigatedPageKey = NavigationService.GetNameOfRegisteredPage(sourcePageType);
            var pageKey = menuItem.GetValue(NavHelper.NavigateToProperty) as string;
            return pageKey == navigatedPageKey;
        }

        private static KeyboardAccelerator BuildKeyboardAccelerator(VirtualKey key, VirtualKeyModifiers? modifiers = null)
        {
            var keyboardAccelerator = new KeyboardAccelerator() { Key = key };
            if (modifiers.HasValue)
            {
                keyboardAccelerator.Modifiers = modifiers.Value;
            }

            keyboardAccelerator.Invoked += OnKeyboardAcceleratorInvoked;
            return keyboardAccelerator;
        }

        private static void OnKeyboardAcceleratorInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            var result = NavigationService.GoBack();
            args.Handled = result;
        }
    }
}
