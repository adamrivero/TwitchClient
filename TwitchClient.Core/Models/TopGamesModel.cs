using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TwitchClient.Core
{
    public class TopGamesModel : INotifyPropertyChanged
    {
        public List<Datum> data { get; set; }
        public Pagination pagination { get; set; }
        private string id;
        private string name;
        private string box_art_url;
        public string Id
        {
            get { return id; }
            set
            {
                id = value;
                OnPropertyChanged("Id");
            }
        }
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                OnPropertyChanged("Name");
            }
        }
        public string Box_art_url
        {
            get { return box_art_url; }
            set
            {
                box_art_url = value;
                OnPropertyChanged("Box_art_url");
            }
        }
        public class Datum
        {
            public string id { get; set; }
            public string name { get; set; }
            public string box_art_url { get; set; }
        }

        public class Pagination
        {
            public string cursor { get; set; }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
