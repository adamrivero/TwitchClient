using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using TwitchClient.Core;
using TwitchClient.Services;
using TwitchClient.Views;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

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
            localData = ApplicationData.Current.LocalSettings;
            if ((string)localData.Values["Search_type"] == "Category")
            {
                searchParam = (string)localData.Values["Game_name"];
            }
            else
            {
                searchParam = (string)localData.Values["Search_param"];
            }
            streamModels = new ObservableCollection<StreamModel>();
            API = new ApiRequest();
            GetStreams(searchParam);
        }

        public async void ClickCommand(object sender, object parameter)
        {
            var arg = parameter as ItemClickEventArgs;
            var item = arg.ClickedItem as StreamModel;
            var UserLogin = await API.GetUserInfoAsync(item.Id);
            localData.Values["User_login"] = UserLogin.data.First().login;
            NavigationService.Navigate("TwitchClient.ViewModels.MediaViewModel");
        }
        private async void GetStreams(string param)
        {
            var message = new MessageDialog("Не понятно что искать, введите запрос", "Упс");
            if (param == null) await message.ShowAsync();
            else
            {
                var streams = await API.SearchStream(param);
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
}
