using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using TwitchClient.Core;
using TwitchClient.Services;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace TwitchClient.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly ApplicationDataContainer localData;
        private readonly ApiRequest api;
        private bool isLoading;
        private string searchParam;

        public MainViewModel()
        {
            IsLoading = true;
            localData = ApplicationData.Current.LocalSettings;
            api = new ApiRequest();
            TopGameModels = new ObservableCollection<TopGamesModel>();
            StreamModels = new ObservableCollection<StreamModel>();
            SearchCommand = new RelayCommand(SearchStream);
            GetStreams();
        }

        public static NavigationServiceEx NavigationService => ViewModelLocator.Current.NavigationService;

        public ObservableCollection<TopGamesModel> TopGameModels { get; private set; }

        public ObservableCollection<StreamModel> StreamModels { get; private set; }

        public bool IsLoading
        {
            get => isLoading;
            set
            {
                isLoading = value;
                RaisePropertyChanged("IsLoading");
            }
        }

        public string SearchParam
        {
            get => searchParam;
            set
            {
                searchParam = value;
                RaisePropertyChanged("SearchParam");
            }
        }

        public ICommand SearchCommand { get; private set; }

        public void SearchStream()
        {
            localData.Values["Search_param"] = SearchParam;
        }

        public async void ClickCommand(object sender, object parameter)
        {
            ItemClickEventArgs arg = parameter as ItemClickEventArgs;
            StreamModel item = arg.ClickedItem as StreamModel;
            UserModel userLogin = await api.GetUserInfoAsync(item.Id);
            localData.Values["User_login"] = userLogin.data.First().login;
            NavigationService.Navigate("TwitchClient.ViewModels.MediaViewModel", null, new DrillInNavigationTransitionInfo());
        }

        public void CategoryCommand(object sender, object parameter)
        {
            ItemClickEventArgs arg = parameter as ItemClickEventArgs;
            TopGamesModel item = arg.ClickedItem as TopGamesModel;
            localData.Values["Game_name"] = item.Name;
            localData.Values["Search_type"] = "Category";
            NavigationService.Navigate("TwitchClient.ViewModels.SearchViewModel", null, new DrillInNavigationTransitionInfo());
        }

        private async void GetStreams()
        {
            SearchStreamModel streams = await api.GetKrakenStreams();
            TopGamesModel topGames = await api.GetTopGameAsync();
            foreach (SearchStreamModel.Stream stream in streams.streams)
            {
                    StreamModels.Add(new StreamModel
                    {
                        Profile_logo = stream.channel.logo,
                        Thumbnail_url = stream.preview.large,
                        Title = stream.channel.status,
                        Game_name = stream.game,
                        User_name = stream.channel.display_name,
                        Viewer_count = stream.viewers,
                        Id = stream.channel._id.ToString(),
                    });
            }

            foreach (TopGamesModel.Datum topGame in topGames.data)
            {
                string set_atr_size = topGame.box_art_url.Replace("{width}", "188");
                set_atr_size = set_atr_size.Replace("{height}", "250");
                TopGameModels.Add(new TopGamesModel { Box_art_url = set_atr_size, Id = topGame.id, Name = topGame.name });
            }

            IsLoading = false;
        }
    }
}
