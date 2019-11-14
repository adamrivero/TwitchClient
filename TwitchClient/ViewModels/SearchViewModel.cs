using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using TwitchClient.Core;
using TwitchClient.Services;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace TwitchClient.ViewModels
{
    public class SearchViewModel : ViewModelBase
    {
        public static NavigationServiceEx NavigationService => ViewModelLocator.Current.NavigationService;
        readonly ApiRequest API;
        ApplicationDataContainer localData;
        private string searchParam;
        public ObservableCollection<StreamModel> streamModels { get; private set; }
        public SearchViewModel()
        {
            
            
            streamModels = new ObservableCollection<StreamModel>();
            API = new ApiRequest();
            GetStreams();
        }


        public async void ClickCommand(object sender, object parameter)
        {
            var arg = parameter as ItemClickEventArgs;
            var item = arg.ClickedItem as StreamModel;
            var UserLogin = await API.GetUserInfoAsync(item.Id);
            localData.Values["User_login"] = UserLogin.data.First().login;
            NavigationService.Navigate("TwitchClient.ViewModels.MediaViewModel");
        }
        private async void GetStreams()
        {
            var localData = ApplicationData.Current.LocalSettings;
            if ((string)localData.Values["Search_param"] != null)
            {
                searchParam = (string)localData.Values["Search_param"];
            }
            else { searchParam = (string)localData.Values["Game_name"]; }
            var streams = await API.SearchStream($"{searchParam}");
            foreach (var stream in streams.streams)
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
        }
    }
}
