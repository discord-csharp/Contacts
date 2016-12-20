using Newtonsoft.Json;
using System.IO;

namespace ContactsBot.Configuration
{
    public class BotConfiguration
    {
        public string Token { get; set; } = "";
        public char PrefixCharacter { get; set; } = '~';
        public string FilterChannel { get; set; } = "";
        public bool EnableLoggingChannel { get; set; } = false;
        public ulong? LoggingGuildID { get; set; } = null;
        public ulong? LoggingChannelID { get; set; } = null;
        public string LoggingNLogLayout { get; set; } = "${time} | **${level}** - ${message}";
        public ulong? MuteRole { get; set; } = null;
        public string MessageOfTheDay { get; set; } = "Hello {0}! Welcome to the server!";
        public string Game { get; set; } = "Helping you C#";
    }
}
