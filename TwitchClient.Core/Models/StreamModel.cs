using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TwitchClient.Core
{
    public class StreamModel : INotifyPropertyChanged
    {
        public List<Datum> data { get; set; }
        private string user_name;
        private int viewer_count;
        private string thumbnail_url;
        private string id;
        private string game_name;
        private string title;
        private string profile_logo;
        public string Profile_logo
        {
            get { return profile_logo; }
            set { profile_logo = value;
                OnPropertyChanged("Profile_logo");
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
        public string Game_name
        {
            get { return game_name; }
            set
            {
                game_name = value;
                OnPropertyChanged("Game_name");
            }
        }
        public string Id
        {
            get { return id; }
            set
            {
                id = value;
                OnPropertyChanged("Id");
            }
        }
        public string User_name
        {
            get { return user_name; }
            set
            {
                user_name = value;
                OnPropertyChanged("User_name");
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
        public string Thumbnail_url
        {
            get { return thumbnail_url; }
            set
            {
                thumbnail_url = value;
                OnPropertyChanged("Thumbnail_url");
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
    public class Datum
    {
        public string id { get; set; }
        public string user_id { get; set; }
        public string user_name { get; set; }
        public int game_id { get; set; }
        public string type { get; set; }
        public string title { get; set; }
        public int viewer_count { get; set; }
        public DateTime started_at { get; set; }
        public string language { get; set; }
        public string thumbnail_url { get; set; }
        public List<string> tag_ids { get; set; }
    }

    public class Pagination
    {
        public string cursor { get; set; }
    }
   
}
