namespace ContactsBot.Configuration
{
    public class ServerConfiguration
    {
        public string PrefixString { get; set; } = "~";

        public bool EnableLoggingChannel { get; set; } = false;

        public ulong LoggingChannelID { get; set; } = 0;

        public string LoggingNLogLayout { get; set; } = "${time} | **${level}** - ${message}";
    }
}
