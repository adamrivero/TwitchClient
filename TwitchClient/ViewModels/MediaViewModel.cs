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
        private Downloader _downloader = null;
        ApplicationDataContainer localData;
        private MediaModel selectedMedia;
        readonly ApiRequest API;
        private string _login;
        TwitchConnection client;
        private Visibility notifyVisibility;
        public Visibility NotifyVisibility
        {
            get { return notifyVisibility; }
            set
            {
                notifyVisibility = value;
                RaisePropertyChanged("NotifyVisibility");
            }
        }
        Thread thread;

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
        public ICommand ProjectionCommand { get; private set; }
        public MediaViewModel()
        {
            client = new TwitchConnection(
                    cluster: ChatEdgeCluster.Aws,
                    nick: "justinfan1",
                    oauth: "sad9di9wad", // no oauth: prefix
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
            localData = ApplicationData.Current.LocalSettings;
            API = new ApiRequest();
            SelectedMedia = new MediaModel();
            Login = (string)localData.Values["User_login"];
            GetStreams();
            StartRecordCommand = new RelayCommand(StartRecord);
            StopRecordCommand = new RelayCommand(StopRecord);
            ProjectionCommand = new RelayCommand(StartProjection);
        }

        private void StartProjection()
        {
            // projection = new ProjectionControls(SelectedMedia.Video_source);
            // projection.Start();
            // Debug.WriteLine("РАБОТАЕТ");
        }

        private  void StartRecord()
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
            //connection.CheckConnection();
            //if (connection.IsConnected)
            //{
            Uri m3u8 = await API.UriAsync(Login);
            var streams = await API.GetStreamInfoAsync($"user_login={Login}");
            foreach (var stream in streams.data)
            {
                var users = await API.GetUserInfoAsync(stream.user_id);
                var games = await API.GetGameInfoAsync(stream.game_id.ToString());
                
                string set_atr_size = games.data.FirstOrDefault(a => a.id == stream.game_id.ToString()).box_art_url.Replace("{width}", "85");
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
                    ChatMessages = new ObservableCollection<ChatModel>()
                };


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
