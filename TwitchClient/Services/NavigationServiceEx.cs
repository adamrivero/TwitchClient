using System;
using System.Collections.Generic;
using System.Linq;

using TwitchClient.Helpers;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace TwitchClient.Services
{
    public class NavigationServiceEx
    {
        public event NavigatedEventHandler Navigated;

        public event NavigationFailedEventHandler NavigationFailed;

        private readonly Dictionary<string, Type> pages = new Dictionary<string, Type>();

        private Frame frame;
        private object lastParamUsed;

        public Frame Frame
        {
            get
            {
                if (frame == null)
                {
                    frame = Window.Current.Content as Frame;
                    RegisterFrameEvents();
                }

                return frame;
            }

            set
            {
                UnregisterFrameEvents();
                frame = value;
                RegisterFrameEvents();
            }
        }

        public bool CanGoBack => Frame.CanGoBack;

        public bool CanGoForward => Frame.CanGoForward;

        public bool GoBack()
        {
            if (CanGoBack)
            {
                Frame.GoBack();
                return true;
            }

            return false;
        }

        public void GoForward() => Frame.GoForward();

        public bool Navigate(string pageKey, object parameter = null, NavigationTransitionInfo infoOverride = null)
        {
            Type page;
            lock (pages)
            {
                if (!pages.TryGetValue(pageKey, out page))
                {
                    throw new ArgumentException(string.Format("ExceptionNavigationServiceExPageNotFound".GetLocalized(), pageKey), nameof(pageKey));
                }
            }

            if (Frame.Content?.GetType() != page || (parameter != null && !parameter.Equals(lastParamUsed)))
            {
                var navigationResult = Frame.Navigate(page, parameter, infoOverride);
                if (navigationResult)
                {
                    lastParamUsed = parameter;
                }

                return navigationResult;
            }
            else
            {
                return false;
            }
        }

        public void Configure(string key, Type pageType)
        {
            lock (pages)
            {
                if (pages.ContainsKey(key))
                {
                    throw new ArgumentException(string.Format("ExceptionNavigationServiceExKeyIsInNavigationService".GetLocalized(), key));
                }

                if (pages.Any(p => p.Value == pageType))
                {
                    throw new ArgumentException(string.Format("ExceptionNavigationServiceExTypeAlreadyConfigured".GetLocalized(), pages.First(p => p.Value == pageType).Key));
                }

                pages.Add(key, pageType);
            }
        }

        public string GetNameOfRegisteredPage(Type page)
        {
            lock (pages)
            {
                if (pages.ContainsValue(page))
                {
                    return pages.FirstOrDefault(p => p.Value == page).Key;
                }
                else
                {
                    throw new ArgumentException(string.Format("ExceptionNavigationServiceExPageUnknown".GetLocalized(), page.Name));
                }
            }
        }

        private void RegisterFrameEvents()
        {
            if (frame != null)
            {
                frame.Navigated += Frame_Navigated;
                frame.NavigationFailed += Frame_NavigationFailed;
            }
        }

        private void UnregisterFrameEvents()
        {
            if (frame != null)
            {
                frame.Navigated -= Frame_Navigated;
                frame.NavigationFailed -= Frame_NavigationFailed;
            }
        }

        private void Frame_NavigationFailed(object sender, NavigationFailedEventArgs e) => NavigationFailed?.Invoke(sender, e);

        private void Frame_Navigated(object sender, NavigationEventArgs e) => Navigated?.Invoke(sender, e);
    }
}
