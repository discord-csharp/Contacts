using Discord.Commands;
using Discord.WebSocket;

namespace ContactsBot.ActionServices
{
    public abstract class ActionService
    {
        protected DiscordSocketClient _client { get; }

        public abstract void Enable();

        public abstract void Disable();

        public ActionService(IDependencyMap map)
        {
            _client = map.Get<DiscordSocketClient>();
        }
    }
}
