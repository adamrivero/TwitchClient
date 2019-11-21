using GalaSoft.MvvmLight;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using TwitchClient.Core;
using TwitchClient.Services;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace TwitchClient.ViewModels
{
    public class SearchViewModel : ViewModelBase
    {

        private readonly ApiRequest api;
        private readonly ApplicationDataContainer localData;
        private readonly string searchParam;

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

            StreamModels = new ObservableCollection<StreamModel>();
            api = new ApiRequest();
            GetStreams(searchParam);
        }

        public static NavigationServiceEx NavigationService => ViewModelLocator.Current.NavigationService;

        public ObservableCollection<StreamModel> StreamModels { get; private set; }

        public async void ClickCommand(object sender, object parameter)
        {
            ItemClickEventArgs arg = parameter as ItemClickEventArgs;
            StreamModel item = arg.ClickedItem as StreamModel;
            UserModel userLogin = await api.GetUserInfoAsync(item.Id);
            localData.Values["User_login"] = userLogin.data.First().login;
            NavigationService.Navigate("TwitchClient.ViewModels.MediaViewModel");
        }

        private async void GetStreams(string param)
        {
            MessageDialog message = new MessageDialog("Не понятно что искать, введите запрос", "Упс");
            if (param == null || param == string.Empty)
            {
                await message.ShowAsync();
            }
            else
            {
                SearchStreamModel streams = await api.SearchStream(param);
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
            }
        }
    }
}
