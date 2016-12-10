using Discord.WebSocket;
using NLog;
using NLog.Targets;
using System;

namespace ContactsBot.NLogTargets
{
    public class DiscordNLogTarget : TargetWithLayout
    {
        private ISocketMessageChannel _logChannel;
        private DiscordSocketClient _socket;
        private static Logger discordNLogTargetLogger = LogManager.GetCurrentClassLogger();
        public DiscordNLogTarget(ulong logGuildID, ulong logChannelID, DiscordSocketClient socket)
        {
            Name = GetType().Name;
            _socket = socket;
            _logChannel = _socket.GetGuild(logGuildID)?.GetChannel(logChannelID) as ISocketMessageChannel;
            if (_logChannel == null)
                throw new ArgumentException("Log Channel for Discord NLog Target could not be found!");
        }

        protected override void Write(LogEventInfo info)
        {
            _logChannel.SendMessageAsync(Layout.Render(info)).Wait();
        }
    }
}
