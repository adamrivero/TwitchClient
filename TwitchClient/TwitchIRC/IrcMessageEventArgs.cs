using System;

namespace TwitchClient.TwitchIRC
{
    public class IrcMessageEventArgs : EventArgs
    {
        public IrcMessage Message { get; private set; }

        public IrcMessageEventArgs(IrcMessage message)
            : base()
        {
            Message = message;
        }
    }
}
