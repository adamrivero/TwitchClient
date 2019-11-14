using System;

namespace TwitchClient.TwitchIRC
{
    public class IrcConnectedEventArgs : EventArgs
    {
        public string Endpoint { get; private set; }

        public IrcConnectedEventArgs(string endpoint)
            : base()
        {
            Endpoint = endpoint;
        }
    }
}
