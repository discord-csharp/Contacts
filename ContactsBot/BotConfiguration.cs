using Newtonsoft.Json;
using System.IO;

namespace ContactsBot
{
    public class BotConfiguration
    {
        public string Token { get; set; }
        public char PrefixCharacter { get; set; } = '~';
        public string FilterChannel { get; set; }
        public string LoggingChannel { get; set; } = "log";
        public string MessageOfTheDay { get; set; } = "Hello {0}! Welcome to the server!";
    }
}
