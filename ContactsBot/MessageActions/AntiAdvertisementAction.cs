using System.Collections.Generic;
using System.Linq;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Net;

namespace ContactsBot.Modules
{
    class AntiAdvertisementAction : IMessageAction
    {
        private DiscordSocketClient _client;

        public bool IsEnabled { get; private set; }

        public void Install(IDependencyMap map)
        {
            _client = map.Get<DiscordSocketClient>();
        }

        public void Enable()
        {
            _client.MessageReceived += RunFilterAsync;
            IsEnabled = true;
        }

        public void Disable()
        {
            _client.MessageReceived -= RunFilterAsync;
            IsEnabled = false;
        }

        public async Task RunFilterAsync(IMessage message)
        {
            var authorAsGuildUser = (message.Author as IGuildUser);
            var guildChannel = message.Channel as IGuildChannel;
            if (guildChannel == null) return;

            if (!authorAsGuildUser.IsCorrectRole(guildChannel.Guild, ModerationModule.StandardRoles))
            {
                if (ContainsInviteLink(message.Content))
                {
                    await message.Channel.DeleteMessagesAsync(new[] { message });
                    var dmChannel = await authorAsGuildUser.CreateDMChannelAsync();
                    await dmChannel.SendMessageAsync("Your Discord invite link was removed. Please ask a staff member or regular to post it.");
                }
            }
        }
        
        private bool ContainsInviteLink(string message)
        {
            var urlRegex = new Regex(@"(http|ftp|https)://([\w_-]+(?:(?:\.[\w_-]+)+))([\w.,@?^=%&:/~+#-]*[\w@?^=%&/~+#-])?", RegexOptions.IgnoreCase);
            var matches = urlRegex.Matches(message);
            var requests = new List<Task<bool>>();

            foreach(Match match in matches)
            {
                if (match.Groups[0].Value.Contains("discord.gg"))
                {
                    return true;
                }
                requests.Add(IsHiddenInviteAsync(match.Groups[0].Value));
            }

            Task.WaitAll(requests.ToArray());

            return requests.Where(x => x.GetAwaiter().GetResult()).Count() != 0;
        }

        private async Task<bool> IsHiddenInviteAsync(string link)
        {
            var client = new HttpClient();

            var res = await client.GetAsync(link, HttpCompletionOption.ResponseHeadersRead);
            if (res.StatusCode == HttpStatusCode.OK)
            {
                return res.RequestMessage.RequestUri.AbsoluteUri.Contains("discordapp.com/invite");
            }
            return false;
        }
    }
}
