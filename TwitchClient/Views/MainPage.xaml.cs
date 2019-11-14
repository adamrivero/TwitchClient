using System;

using TwitchClient.ViewModels;

using Windows.UI.Xaml.Controls;

namespace TwitchClient.Views
{
    public sealed partial class MainPage : Page
    {
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
