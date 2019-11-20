using System;
using System.Collections.Generic;
using System.Text;

namespace TwitchClient.Core
{
    public class StaticProfileModel
    {
        public int ID { get; set; }
        public string Bio { get; set; }
        public string Display_name { get; set; }
        public string Logo { get; set; }
        public string Name { get; set; }
        public string OAuth { get; set; }
    }
}
