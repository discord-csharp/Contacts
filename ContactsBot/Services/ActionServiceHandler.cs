using Discord.Commands;
using Discord.WebSocket;
using System.Collections.Generic;

namespace ContactsBot.ActionServices
{
    public class ActionServiceHandler
    {
        Dictionary<string, ActionService> _services = new Dictionary<string, ActionService>();
        DiscordSocketClient _client;

        public ActionServiceHandler(IDependencyMap map)
        {
            _client = map.Get<DiscordSocketClient>();
        }
    }
}
