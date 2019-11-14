using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TwitchClient.Core
{
    public class MediaModel : INotifyPropertyChanged
    {
        private Uri video_source;
        private string title;
        private string description;
        private int viewer_count;
        private int view_count;
        private string display_name;
        private string atr_url;
        private string game_name;
        private string login;
        public ObservableCollection<ChatModel> ChatMessages { get; set; }
        public string Login
        {
            get { return login; }
            set
            {
                login = value;
                OnPropertyChanged("Login");
            }
        }
        public string Display_name
        {
            get { return display_name; }
            set
            {
                display_name = value;
                OnPropertyChanged("Display_name");
            }
        }
        public Uri Video_source
        {
            get { return video_source; }
            set
            {
                video_source = value;
                OnPropertyChanged("Video_source");
            }
        }
        public string Title
        {
            get { return title; }
            set
            {
                title = value;
                OnPropertyChanged("Title");
            }
        }
        public int Viewer_count
        {
            get { return viewer_count; }
            set
            {
                viewer_count = value;
                OnPropertyChanged("Viewer_count");
            }
        }
        public string Description
        {
            get { return description; }
            set
            {
                description = value;
                OnPropertyChanged("Description");
            }
        }
        public int View_count
        {
            get { return view_count; }
            set
            {
                view_count = value;
                OnPropertyChanged("View_count");
            }
        }
        public string Atr_url
        {
            get { return atr_url; }
            set
            {
                atr_url = value;
                OnPropertyChanged("Atr_url");
            }
        }
        public string Game_name
        {
            get { return game_name; }
            set
            {
                game_name = value;
                OnPropertyChanged("Game_name");
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
