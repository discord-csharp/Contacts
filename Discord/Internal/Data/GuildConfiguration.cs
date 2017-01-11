using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContactsBot.Data
{
    [Table("GuildConfigurations")]
    public class GuildConfiguration
    {
        public string PrefixString { get; set; } = "~";

        public bool EnableLoggingChannel { get; set; } = false;

        public ulong LoggingChannelID { get; set; } = 0;

        public string LoggingNLogLayout { get; set; } = "${time} | **${level}** - ${message}";
    }
}
