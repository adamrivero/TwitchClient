using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Input;
using TwitchClient.Core;
using TwitchClient.Helpers;
using TwitchClient.TwitchIRC;
using Windows.ApplicationModel.DataTransfer;
using Windows.Globalization;
using Windows.Storage;
using Windows.UI.StartScreen;
using Windows.UI.Xaml;

namespace TwitchClient.ViewModels
{
    public class MediaViewModel : ViewModelBase
    {
        // ProjectionControls projection;
        // InternetConnection connection;
        private readonly Thread thread;
        private readonly ApiRequest api;
        private readonly TwitchConnection client;
        private Downloader downloader = null;
        private ApplicationDataContainer localData;
        private MediaModel selectedMedia;
        private bool isLoading;
        private string login;
        private string channelID;
        private string chatMessage;
        private Visibility notifyVisibility;

        public MediaViewModel()
        {
            this.localData = ApplicationData.Current.LocalSettings;
            this.client = new TwitchConnection(
                    cluster: ChatEdgeCluster.Aws,
                    nick: "zlistiev",
                    oauth: (string)this.localData.Values["OAuth"], // no oauth: prefix
                    port: 6697,
                    capRequests: new string[] { "twitch.tv/tags", "twitch.tv/commands" },
                    ratelimit: 1500,
                    secure: true);
            this.thread = new Thread(() =>
            {
                this.client.Connect();
            });
            this.NotifyVisibility = Visibility.Collapsed;

            // connection = new InternetConnection();
            ApplicationLanguages.PrimaryLanguageOverride = "en-US";
            this.api = new ApiRequest();
            this.SelectedMedia = new MediaModel();
            this.channelID = (string)localData.Values["ChannelID"];
            this.Login = (string)localData.Values["User_login"];
            this.GetStreams();
            this.StartRecordCommand = new RelayCommand(StartRecord);
            this.StopRecordCommand = new RelayCommand(StopRecord);
            this.SendMessageCommand = new RelayCommand(SendChatMessage);
            this.FollowCommand = new RelayCommand(FollowChannel);
            this.ShareCommand = new RelayCommand(ShareContact);
            this.TileCommand = new RelayCommand(PinTile);
        }

        public string ChatMessage
        {
            get
            {
                return this.chatMessage;
            }

            set
            {
                this.chatMessage = value;
                this.RaisePropertyChanged("ChatMessage");
            }
        }

        public Visibility NotifyVisibility
        {
            get
            {
                return this.notifyVisibility;
            }

            set
            {
                this.notifyVisibility = value;
                this.RaisePropertyChanged("NotifyVisibility");
            }
        }

        public bool IsLoading
        {
            get
            {
                return this.isLoading;
            }

            set
            {
                this.isLoading = value;
                this.RaisePropertyChanged("IsLoading");
            }
        }

        public string Login
        {
            get
            {
                return this.login;
            }

            set
            {
                this.login = value;
                this.RaisePropertyChanged("Login");
            }
        }

        public MediaModel SelectedMedia
        {
            get
            {
                return this.selectedMedia;
            }

            set
            {
                this.selectedMedia = value;
                this.RaisePropertyChanged("SelectedMedia");
            }
        }

        public ICommand HomeCommand { get; private set; }

        public ICommand StartRecordCommand { get; private set; }

        public ICommand StopRecordCommand { get; private set; }

        public ICommand SendMessageCommand { get; private set; }

        public ICommand FollowCommand { get; private set; }

        public ICommand ShareCommand { get; private set; }

        public ICommand TileCommand { get; private set; }

        private async void PinTile()
        {
            SecondaryTile tile = new SecondaryTile(
                "myTileId5391",
                SelectedMedia.Login,
                "Last stream",
                new Uri("ms-appx:///Assets/tile-sdk.png"),
                TileSize.Default);
            tile.VisualElements.ShowNameOnSquare150x150Logo = true;
            tile.VisualElements.ForegroundText = ForegroundText.Light;
            bool isPinned = await tile.RequestCreateAsync();
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
            var result = await api.GetProfileAsync((string)localData.Values["OAuth"]);
            await api.FollowChannelAsync(result._id.ToString(), SelectedMedia.UserID, (string)localData.Values["OAuth"]);
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
            downloader = new Downloader(SelectedMedia.Video_source, selectedMedia.Login, DateTime.Now.Second.ToString());
        }

        private void StopRecord()
        {
            downloader?.Stop();
            NotifyVisibility = Visibility.Collapsed;
            Debug.WriteLine("Record was stopped");
        }

        private async void GetStreams()
        {
            IsLoading = true;

            // connection.CheckConnection();
            // if (connection.IsConnected)
            // {
            Uri m3u8 = await api.UriAsync(Login);
            var streams = await api.GetStreamInfoAsync($"user_login={Login}");
            UserModel users = new UserModel();
            GameModel games = new GameModel();
            foreach (var stream in streams.data)
            {
                users = await api.GetUserInfoAsync(stream.user_id);
                games = await api.GetGameInfoAsync(stream.game_id.ToString());

                string set_atr_size;

                set_atr_size = games.data.FirstOrDefault(a => a.id == stream.game_id.ToString()).box_art_url.Replace("{width}", "85");
                set_atr_size = set_atr_size.Replace("{height}", "113");
                SelectedMedia = new MediaModel
                {
                    Login = users.data[0].login,
                    Atr_url = set_atr_size,
                    Game_name = games.data.FirstOrDefault(a => a.id == stream.game_id.ToString()).name,
                    Video_source = new Uri(await api.ParseM3UAsync(m3u8, "720p60")),
                    Description = users.data.FirstOrDefault(a => a.id == stream.user_id).description,
                    Display_name = users.data.FirstOrDefault(a => a.id == stream.user_id).display_name,
                    Title = stream.title,
                    Viewer_count = stream.viewer_count,
                    View_count = users.data.FirstOrDefault(a => a.id == stream.user_id).view_count,
                    ChatMessages = new ObservableCollection<ChatModel>(),
                    UserID = stream.user_id,
                    Logo = users.data[0].profile_image_url,
                };
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
