using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System.Threading.Tasks;

namespace ContactsBot.Modules
{
    class UserMotd : IMessageAction
    {
        private DiscordSocketClient _client;
        private BotConfiguration _config;

        public bool IsEnabled { get; private set; }

        public void Install(IDependencyMap map)
        {
            _client = map.Get<DiscordSocketClient>();
            _config = map.Get<BotConfiguration>();
        }

        public void Enable()
        {
            _client.UserJoined += ShowMotd;
            IsEnabled = true;
        }

        public void Disable()
        {
            _client.UserJoined -= ShowMotd;
            IsEnabled = false;
        }

        public async Task ShowMotd(SocketGuildUser user)
        {
            var channel = await user.Guild.GetDefaultChannelAsync();

            try
            {
                await channel.SendMessageAsync(String.Format(_config.MessageOfTheDay, user.Mention));
            }
            catch (FormatException)
            {
                await channel.SendMessageAsync("Tell the admin to fix the MOTD formatting!");
            }
        }
    }
}
