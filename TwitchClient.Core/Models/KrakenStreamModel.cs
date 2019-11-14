﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchClient.Core
{
    public class KrakenStreamModel
    {
        public int _total { get; set; }
        public List<Stream> streams { get; set; }
        public class Channel
        {
            public int _id { get; set; }
            public string broadcaster_language { get; set; }
            public DateTime created_at { get; set; }
            public string display_name { get; set; }
            public int followers { get; set; }
            public string game { get; set; }
            public string language { get; set; }
            public string logo { get; set; }
            public bool mature { get; set; }
            public string name { get; set; }
            public bool partner { get; set; }
            public string profile_banner { get; set; }
            public object profile_banner_background_color { get; set; }
            public string status { get; set; }
            public DateTime updated_at { get; set; }
            public string url { get; set; }
            public string video_banner { get; set; }
            public int views { get; set; }
        }

        public class Preview
        {
            public string large { get; set; }
            public string medium { get; set; }
            public string small { get; set; }
            public string template { get; set; }
        }

        public class Stream
        {
            public long _id { get; set; }
            public int average_fps { get; set; }
            public Channel channel { get; set; }
            public DateTime created_at { get; set; }
            public int delay { get; set; }
            public string game { get; set; }
            public bool is_playlist { get; set; }
            public Preview preview { get; set; }
            public int video_height { get; set; }
            public int viewers { get; set; }
        }
    }
}