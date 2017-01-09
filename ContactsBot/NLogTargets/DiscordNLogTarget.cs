using Discord;
using Discord.WebSocket;
using NLog;
using NLog.Targets;
using System;

namespace ContactsBot.NLogTargets
{
    public class DiscordNLogTarget : TargetWithLayout
    {
        private ITextChannel _logChannel;

        private Logger DiscordNLogTargetLogger { get; }

        public DiscordNLogTarget(ITextChannel logChannel, DiscordSocketClient discordClient)
        {
            DiscordNLogTargetLogger = LogManager.GetLogger(logChannel.Id.ToString());
            Name = GetType().Name;
            _logChannel = logChannel;
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
