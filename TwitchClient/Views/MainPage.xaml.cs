using System;
using TwitchClient.Controls;
using TwitchClient.ViewModels;

using Windows.UI.Xaml.Controls;

namespace TwitchClient.Views
{
    public sealed partial class MainPage : Page
    {
        public static MainPage Current;
        public ViewLifetimeControl ProjectionViewPageControl;
        private MainViewModel ViewModel
        {
            get { return ViewModelLocator.Current.MainViewModel; }
        }
        public MainPage()
        {
            InitializeComponent();
            DataContext = ViewModel;
        }
    }
}
