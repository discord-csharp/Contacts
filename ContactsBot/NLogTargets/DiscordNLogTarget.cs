using Discord.WebSocket;
using NLog;
using NLog.Targets;
using System;

namespace ContactsBot.NLogTargets
{
    public class DiscordNLogTarget : TargetWithLayout
    {
        private ISocketMessageChannel _logChannel;
        private static Logger DiscordNLogTargetLogger { get; } = LogManager.GetCurrentClassLogger();
        public DiscordNLogTarget(ulong logGuildID, ulong logChannelID, DiscordSocketClient discordClient)
        {
            Name = GetType().Name;
            _logChannel = discordClient.GetGuild(logGuildID)?.GetChannel(logChannelID) as ISocketMessageChannel;
            if (_logChannel == null)
            {
                DiscordNLogTargetLogger.Error("Log Channel for Discord NLog Target could not be found!");
                throw new ArgumentException("Log Channel for Discord NLog Target could not be found!");
            }
        }

        protected override void Write(LogEventInfo info)
        {
            _logChannel.SendMessageAsync(Layout.Render(info)).Wait();
        }
    }
}
