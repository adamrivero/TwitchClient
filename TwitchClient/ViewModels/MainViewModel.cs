using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using TwitchClient.Core;
using TwitchClient.Services;
using TwitchClient.Views;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace TwitchClient.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public static NavigationServiceEx NavigationService => ViewModelLocator.Current.NavigationService;
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
        ApplicationDataContainer localData;
        readonly ApiRequest API;
        public ObservableCollection<TopGamesModel> topGameModels { get; private set; }
        public ObservableCollection<StreamModel> streamModels { get; private set; }
        public ICommand SearchCommand { get; private set; }
        public MainViewModel()
        {
            localData = ApplicationData.Current.LocalSettings;
            API = new ApiRequest();
            topGameModels = new ObservableCollection<TopGamesModel>();
            streamModels = new ObservableCollection<StreamModel>();
            SearchCommand = new RelayCommand(SearchStream);
            GetStreams();
        }
        public void Initialize()
        {
            Debug.WriteLine("Hello");
        }
        public void SearchStream()
        {
            localData.Values["Search_param"] = SearchParam;
        }
        public async void ClickCommand(object sender, object parameter)
        {
            var arg = parameter as ItemClickEventArgs;
            var item = arg.ClickedItem as StreamModel;
            var UserLogin = await API.GetUserInfoAsync(item.Id);
            localData.Values["User_login"] = UserLogin.data.First().login;
            NavigationService.Navigate("TwitchClient.ViewModels.MediaViewModel");
        }
        public void CategoryCommand(object sender, object parameter)
        {
            var arg = parameter as ItemClickEventArgs;
            var item = arg.ClickedItem as TopGamesModel;
            localData.Values["Game_name"] = item.Name;
            NavigationService.Navigate("TwitchClient.ViewModels.SearchViewModel");
        }
        private async void GetStreams()
        {
            var streams = await API.GetKrakenStreams();
            var TopGames = await API.GetTopGameAsync();
            foreach (var stream in streams.streams)
            {
                try
                {
                    streamModels.Add(new StreamModel
                    {
                        Profile_logo = stream.channel.logo,
                        Thumbnail_url = stream.preview.large,
                        Title = stream.channel.status,
                        Game_name = stream.game,
                        User_name = stream.channel.display_name,
                        Viewer_count = stream.viewers,
                        Id = stream.channel._id.ToString()
                    });
                }
                catch
                {
                    streamModels.Add(new StreamModel
                    {
                        Profile_logo = stream.channel.logo,
                        Thumbnail_url = stream.preview.medium,
                        Title = stream.channel.status,
                        Game_name = stream.game,
                        User_name = stream.channel.display_name,
                        Viewer_count = stream.viewers,
                        Id = stream.channel._id.ToString()
                    });
                }
            }
            foreach (var TopGame in TopGames.data)
            {
                string set_atr_size = TopGame.box_art_url.Replace("{width}", "188");
                set_atr_size = set_atr_size.Replace("{height}", "250");
                topGameModels.Add(new TopGamesModel { Box_art_url = set_atr_size, Id = TopGame.id, Name = TopGame.name });
            }
        }
    }
}
