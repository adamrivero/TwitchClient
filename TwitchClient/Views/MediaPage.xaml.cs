using System;

using TwitchClient.ViewModels;

using Windows.UI.Xaml.Controls;

namespace TwitchClient.Views
{
    public sealed partial class MediaPage : Page
    {
        private MediaViewModel ViewModel
        {
            get { return ViewModelLocator.Current.MediaViewModel; }
        }

        public MediaPage()
        {
            InitializeComponent();
            DataContext = ViewModel;
        }
    }
}
