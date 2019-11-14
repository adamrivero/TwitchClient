using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TwitchClient.Core
{
    public class AllInfo : INotifyPropertyChanged
    {
        private string _id;
        private string _login;
        private string _display_name;
        private string _type;
        private string _broadcaster_type;
        private string _description;
        private string _profile_image_url;
        private string _top_game_name;
        private string _offline_image_url;
        private int _view_count;
        private string _email;
        private string _user_id;
        private string _user_name;
        private int? _game_id;
        private string _title;
        private int _viewer_count;
        private DateTime _started_at;
        private string _language;
        private string _thumbnail_url;
        private Uri _streamVideoURI;
        private string _game_name;
        private string _box_art_url;
        public string top_game_name
        {
            get { return _top_game_name; }
            set
            {
                _top_game_name = value;
                OnPropertyChanged("top_game_name");
            }
        }
        public string id
        {
            get { return _id; }
            set
            {
                _id = value;
                OnPropertyChanged("id");
            }
        }
        public string login
        {
            get { return _login; }
            set
            {
                _login = value;
                OnPropertyChanged("login");
            }
        }
        public string display_name
        {
            get { return _display_name; }
            set
            {
                _display_name = value;
                OnPropertyChanged("display_name");
            }
        }
        public string type
        {
            get { return _type; }
            set
            {
                _type = value;
                OnPropertyChanged("type");
            }
        }
        public string broadcaster_type
        {
            get { return _broadcaster_type; }
            set
            {
                _broadcaster_type = value;
                OnPropertyChanged("broadcaster_type");
            }
        }
        public string description
        {
            get { return _description; }
            set
            {
                _description = value;
                OnPropertyChanged("description");
            }
        }
        public string profile_image_url
        {
            get { return _profile_image_url; }
            set
            {
                _profile_image_url = value;
                OnPropertyChanged("profile_image_url");
            }
        }
        public string offline_image_url
        {
            get { return _offline_image_url; }
            set
            {
                _offline_image_url = value;
                OnPropertyChanged("offline_image_url");
            }
        }
        public int view_count
        {
            get { return _view_count; }
            set
            {
                _view_count = value;
                OnPropertyChanged("view_count");
            }
        }
        public string email
        {
            get { return _email; }
            set
            {
                _email = value;
                OnPropertyChanged("email");
            }
        }
        public string user_id
        {
            get { return _user_id; }
            set
            {
                _user_id = value;
                OnPropertyChanged("user_id");
            }
        }
        public string user_name
        {
            get { return _user_name; }
            set
            {
                _user_name = value;
                OnPropertyChanged("user_name");
            }
        }
        public int? game_id
        {
            get { return _game_id; }
            set
            {
                _game_id = value;
                OnPropertyChanged("_ame_id");
            }
        }
        public string title
        {
            get { return _title; }
            set
            {
                _title = value;
                OnPropertyChanged("title");
            }
        }
        public int viewer_count
        {
            get { return _viewer_count; }
            set
            {
                _viewer_count = value;
                OnPropertyChanged("viewer_count");
            }
        }
        public DateTime started_at
        {
            get { return _started_at; }
            set
            {
                _started_at = value;
                OnPropertyChanged("title");
            }
        }
        public string language
        {
            get { return _language; }
            set
            {
                _language = value;
                OnPropertyChanged("language");
            }
        }
        public string thumbnail_url
        {
            get { return _thumbnail_url; }
            set
            {
                _thumbnail_url = value;
                OnPropertyChanged("thumbnail_url");
            }
        }
        public Uri streamVideoURI
        {
            get { return _streamVideoURI; }
            set
            {
                _streamVideoURI = value;
                OnPropertyChanged("streamVideoURI");
            }
        }
        public string game_name
        {
            get { return _game_name; }
            set
            {
                _game_name = value;
                OnPropertyChanged("game_name");
            }
        }
        public string box_art_url
        {
            get { return _box_art_url; }
            set
            {
                _box_art_url = value;
                OnPropertyChanged("box_art_url");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
