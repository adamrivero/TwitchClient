using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using TwitchClient.Core;
using TwitchClient.Helpers;
using TwitchClient.TwitchIRC;
using Windows.ApplicationModel.DataTransfer;
using Windows.Globalization;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace TwitchClient.ViewModels
{
    public class MediaViewModel : ViewModelBase
    {
        //ProjectionControls projection;
        //InternetConnection connection;
        Thread thread;
        private Downloader _downloader = null;
        ApplicationDataContainer localData;
        private MediaModel selectedMedia;
        readonly ApiRequest API;
        private string _login;
        TwitchConnection client;
        public string ChannelID;
        private string chatMessage;
        private Visibility notifyVisibility;
        public string ChatMessage
        {
            get { return chatMessage; }
            set
            {
                chatMessage = value;
                RaisePropertyChanged("ChatMessage");
            }
        }
        public Visibility NotifyVisibility
        {
            get { return notifyVisibility; }
            set
            {
                notifyVisibility = value;
                RaisePropertyChanged("NotifyVisibility");
            }
        }
        private bool isLoading;
        public bool IsLoading
        {
            get { return isLoading; }
            set
            {
                isLoading = value;
                RaisePropertyChanged("IsLoading");
            }
        }

        public string Login
        {
            get { return _login; }
            set
            {
                _login = value;
                RaisePropertyChanged("Login");
            }
        }
        public MediaModel SelectedMedia
        {
            get { return selectedMedia; }
            set
            {
                selectedMedia = value;
                RaisePropertyChanged("SelectedMedia");
            }
        }
        public ICommand HomeCommand { get; private set; }
        public ICommand StartRecordCommand { get; private set; }
        public ICommand StopRecordCommand { get; private set; }
        public ICommand SendMessageCommand { get; private set; }
        public ICommand FollowCommand { get; private set; }
        public ICommand ShareCommand { get; private set; }
        public MediaViewModel()
        {
            localData = ApplicationData.Current.LocalSettings;
            client = new TwitchConnection(
                    cluster: ChatEdgeCluster.Aws,
                    nick: "zlistiev",
                    oauth: (string)localData.Values["OAuth"], // no oauth: prefix
                    port: 6697,
                    capRequests: new string[] { "twitch.tv/tags", "twitch.tv/commands" },
                    ratelimit: 1500,
                    secure: true
                    );
            thread = new Thread(() =>
            {
                client.Connect();
            });
            NotifyVisibility = Visibility.Collapsed;
            //connection = new InternetConnection();
            ApplicationLanguages.PrimaryLanguageOverride = "en-US";
            API = new ApiRequest();
            SelectedMedia = new MediaModel();
            ChannelID = (string)localData.Values["ChannelID"];
            Login = (string)localData.Values["User_login"];
            GetStreams();
            StartRecordCommand = new RelayCommand(StartRecord);
            StopRecordCommand = new RelayCommand(StopRecord);
            SendMessageCommand = new RelayCommand(SendChatMessage);
            FollowCommand = new RelayCommand(FollowChannel);
            ShareCommand = new RelayCommand(ShareContact);
        }

        private void ShareContact()
        {
            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += DataTransferManager_DataRequested;
            DataTransferManager.ShowShareUI();

        }

        private void DataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            DataRequest request = args.Request;
            request.Data.SetWebLink(new Uri("https://twitch.tv/" + SelectedMedia.Login));
            request.Data.Properties.Title = "Поделиться ссылкой";
            request.Data.Properties.Description = "Вы можете поделиться ссылкой на трансляцию";
        }

        private async void FollowChannel()
        {
            var result = await API.GetProfileAsync((string)localData.Values["OAuth"]);
            await API.FollowChannelAsync(result._id.ToString(), SelectedMedia.UserID, (string)localData.Values["OAuth"]);
        }

        private void SendChatMessage()
        {
            client.SendMessage("#" + SelectedMedia.Login, ChatMessage);
            SelectedMedia.ChatMessages.Add(new ChatModel { User = $"zlistiev", Message = $"{chatMessage}" });

            Debug.WriteLine("Ну отправил");
        }

        private void StartRecord()
        {
            NotifyVisibility = Visibility.Visible;
            _downloader = new Downloader(SelectedMedia.Video_source, selectedMedia.Login, DateTime.Now.Second.ToString());
        }

        private void StopRecord()
        {
            _downloader?.Stop();
            NotifyVisibility = Visibility.Collapsed;
            Debug.WriteLine("Record was stopped");
        }

        private async void GetStreams()
        {
            IsLoading = true;

            //connection.CheckConnection();
            //if (connection.IsConnected)
            //{
            Uri m3u8 = await API.UriAsync(Login);
            var streams = await API.GetStreamInfoAsync($"user_login={Login}");
            UserModel users = new UserModel();
            GameModel games = new GameModel();
            foreach (var stream in streams.data)
            {
                Thread thread1 = new Thread(async () =>
                {
                    users = await API.GetUserInfoAsync(stream.user_id);
                    games = await API.GetGameInfoAsync(stream.game_id.ToString());
                });
                thread1.Start();
                string set_atr_size;
                load();
                async void load()
                {
                    if (games.data == null || users.data == null)
                    {
                        Thread.Sleep(100);
                        load();
                    }
                    else
                    {
                        set_atr_size = games.data.FirstOrDefault(a => a.id == stream.game_id.ToString()).box_art_url.Replace("{width}", "85");
                        set_atr_size = set_atr_size.Replace("{height}", "113");
                        SelectedMedia = new MediaModel
                        {
                            Login = users.data[0].login,
                            Atr_url = set_atr_size,
                            Game_name = games.data.FirstOrDefault(a => a.id == stream.game_id.ToString()).name,
                            Video_source = new Uri(await API.ParseM3UAsync(m3u8, "720p60")),
                            Description = users.data.FirstOrDefault(a => a.id == stream.user_id).description,
                            Display_name = users.data.FirstOrDefault(a => a.id == stream.user_id).display_name,
                            Title = stream.title,
                            Viewer_count = stream.viewer_count,
                            View_count = users.data.FirstOrDefault(a => a.id == stream.user_id).view_count,
                            ChatMessages = new ObservableCollection<ChatModel>(),
                            UserID = stream.user_id,
                            Logo = users.data[0].profile_image_url
                        };
                    }
                }

                client.Connected += (object sender, IrcConnectedEventArgs e) =>
                {
                    Debug.WriteLine("Connected");
                    client.JoinChannel($"#{SelectedMedia.Login}");
                };
                client.MessageReceived += async (object sender, IrcMessageEventArgs e) =>
                {
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        SelectedMedia.ChatMessages.Add(new ChatModel { User = $"{e.Message.User}", Message = $"{e.Message.Message}" });
                    });
                    Debug.WriteLine($"{e.Message.User.ToString()}: {e.Message.Message.ToString()}");
                };

                client.Reconnected += (object sender, EventArgs e) =>
                {
                    Debug.WriteLine("Reconnected");
                };
                thread.Start();
                IsLoading = false;
            }
            //}
            //else
            //{
            //    var message = new MessageDialog("Проверьте соединение с интернетом", "Упс");
            //    await message.ShowAsync();
            //    Thread.Sleep(2000);
            //    GetStreams();
            //}
        }
    }
}
