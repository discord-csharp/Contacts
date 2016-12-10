using System;
using Discord.WebSocket;
using Discord.Commands;
using System.Threading.Tasks;
using ContactsBot.Configuration;
using NLog;

namespace ContactsBot.Modules
{
    public class UserMotdAction : IMessageAction
    {
        private DiscordSocketClient _client;
        private BotConfiguration _config;
        private static Logger UserMotdLogger { get; set; } = LogManager.GetCurrentClassLogger();

        public bool IsEnabled { get; private set; }

        public void Install(IDependencyMap map)
        {
            _client = map.Get<DiscordSocketClient>();
#if DEV
            _config = map.Get<ConfigManager>().GetConfig<BotConfiguration>(name: "dev").Result;
#else
            _config = map.Get<ConfigManager>().GetConfig<BotConfiguration>().Result;
#endif
        }

        public void Enable()
        {
            _client.UserJoined += ShowMotdAsync;
            IsEnabled = true;
        }

        public void Disable()
        {
            _client.UserJoined -= ShowMotdAsync;
            IsEnabled = false;
        }

        public async Task ShowMotdAsync(SocketGuildUser user)
        {
            var channel = await user.CreateDMChannelAsync();

            try
            {
                await channel.SendMessageAsync(String.Format(_config.MessageOfTheDay, user.Username));
            }
            catch (FormatException)
            {
                UserMotdLogger.Error("Tell the admin to fix the MOTD formatting!");
            }
        }
    }
}
