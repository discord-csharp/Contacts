namespace ContactsBot.Configuration
{
    public class ServerBotConfiguration
    {
        public string PrefixString { get; set; } = "~";
        public bool EnableFilterChannels { get; set; } = false;
        public ulong[] FilterChannels { get; set; } = null;
        public bool EnableLoggingChannel { get; set; } = false;
        public ulong LoggingChannelID { get; set; } = 0;
        public string LoggingNLogLayout { get; set; } = "${time} | **${level}** - ${message}";
        public bool EnableMuteRole { get; set; } = false;
        public ulong MuteRole { get; set; } = 0;
        public string MessageOfTheDay { get; set; } = "Hello {0}! Welcome to the server!";
    }
}
