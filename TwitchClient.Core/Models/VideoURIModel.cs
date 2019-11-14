using System;

namespace TwitchClient.Core
{
    public class VideoURIModel
    {
        public string token { get; set; }
        public string sig { get; set; }
        public bool mobile_restricted { get; set; }
        public DateTime expires_at { get; set; }
    }
}
