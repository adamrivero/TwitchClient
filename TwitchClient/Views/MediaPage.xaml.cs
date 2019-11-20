using System;
using System.Diagnostics;
using TwitchClient.Controls;
using TwitchClient.ViewModels;
using Windows.ApplicationModel.Core;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
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
            DataContext = new MediaViewModel();
        }
    }
}
