using System;

using GalaSoft.MvvmLight.Ioc;

using TwitchClient.Services;
using TwitchClient.Views;

namespace TwitchClient.ViewModels
{
    [Windows.UI.Xaml.Data.Bindable]
    public class ViewModelLocator
    {
        private static ViewModelLocator current;

        private ViewModelLocator()
        {
            SimpleIoc.Default.Register(() => new NavigationServiceEx());
            SimpleIoc.Default.Register<ShellViewModel>();
            Register<MainViewModel, MainPage>();
            Register<MediaViewModel, MediaPage>();
            Register<SearchViewModel, SearchPage>();
        }

        public static ViewModelLocator Current => current ?? (current = new ViewModelLocator());

        public SearchViewModel SearchViewModel => SimpleIoc.Default.GetInstance<SearchViewModel>();

        public MediaViewModel MediaViewModel => SimpleIoc.Default.GetInstance<MediaViewModel>();

        public MainViewModel MainViewModel => SimpleIoc.Default.GetInstance<MainViewModel>();

        public ShellViewModel ShellViewModel => SimpleIoc.Default.GetInstance<ShellViewModel>();

        public NavigationServiceEx NavigationService => SimpleIoc.Default.GetInstance<NavigationServiceEx>();

        public void Register<TVM, TV>()
            where TVM : class
        {
            SimpleIoc.Default.Register<TVM>();

            NavigationService.Configure(typeof(TVM).FullName, typeof(TV));
        }
    }
}
