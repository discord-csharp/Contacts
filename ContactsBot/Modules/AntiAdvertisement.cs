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
    class AntiAdvertisement : IMessageAction
    {
        private DiscordSocketClient _client;

        public bool IsEnabled { get; private set; }

        public void Install(IDependencyMap map)
        {
            _client = map.Get<DiscordSocketClient>();
        }

        public void Enable()
        {
            _client.MessageReceived += RunFilter;
            IsEnabled = true;
        }

        public void Disable()
        {
            _client.MessageReceived -= RunFilter;
            IsEnabled = false;
        }

        public async Task RunFilter(IMessage message)
        {
            var authorAsGuildUser = (message.Author as IGuildUser);
            var guildChannel = message.Channel as IGuildChannel;

            if (!authorAsGuildUser.IsCorrectRole(guildChannel.Guild, Moderation.StandardRoles))
            {
                if (message.Content.Contains("discord.gg"))
                {
                    await message.Channel.DeleteMessagesAsync(new[] { message });
                    var dmChannel = await authorAsGuildUser.CreateDMChannelAsync();
                    await dmChannel.SendMessageAsync("Your Discord invite link was removed. Please ask a staff member or regular to post it.");
                }
            }
        }
    }
}
