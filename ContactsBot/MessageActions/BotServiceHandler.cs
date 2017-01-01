using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ContactsBot.Modules
{
    public class BotServiceHandler
    {
        private IDependencyMap _map { get; }
        private DiscordSocketClient _client { get; }
        private Dictionary<IGuild, List<>>

        public BotServiceHandler(IDependencyMap map)
        {
            _map = map;
            _client = _map.Get<DiscordSocketClient>();
        }
    }
}
