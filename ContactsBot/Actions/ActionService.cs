using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace ContactsBot.ActionServices
{
    public abstract class ActionService
    {
        protected DiscordSocketClient Client { get; }

        public abstract void Invoke(SocketMessage message);

        public ActionService(IDependencyMap map)
        {
            Client = map.Get<DiscordSocketClient>();
        }
    }
}
