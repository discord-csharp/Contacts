using System.Collections.Generic;
using Newtonsoft.Json;

namespace ContactsBot.Configuration
{
    public class ServerConfiguration
    {
        public string PrefixString { get; set; } = "~";

        public bool EnableLoggingChannel { get; set; } = false;

        public ulong LoggingChannelID { get; set; } = 0;

        public string LoggingNLogLayout { get; set; } = "${time} | **${level}** - ${message}";

        public bool EnableMuteRole { get; set; } = false;

        public ulong MuteRole { get; set; } = 0;

        public string MessageOfTheDay { get; set; } = "Hello {0}! Welcome to the server!";
        
        private List<string> _enabledActions { get; set; }
    }
}
