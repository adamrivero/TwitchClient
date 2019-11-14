using System;

using TwitchClient.ViewModels;

using Windows.UI.Xaml.Controls;

namespace TwitchClient.Views
{
    public sealed partial class SearchPage : Page
    {
        private SearchViewModel ViewModel
        {
            get { return ViewModelLocator.Current.SearchViewModel; }
        }

        public SearchPage()
        {
            InitializeComponent();
            DataContext = ViewModel;
        }
    }
}
