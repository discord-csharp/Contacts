using ContactsBot.Configuration;
using Discord;
using Discord.Commands;

namespace ContactsBot
{
    public class ServerCommandContext : ICommandContext
    {
        public IMessageChannel Channel { get; }

        public IDiscordClient Client { get; }

        public IGuild Guild { get; }

        public IUserMessage Message { get; }

        public IUser User { get; }

        public bool IsPrivate => Channel is IPrivateChannel;

        public ServerConfiguration Configuration { get; }

        public ServerCommandContext(IDiscordClient client, IUserMessage message, ServerConfiguration config)
        {
            Channel = message.Channel;
            Client = client;
            Guild = (message.Channel as IGuildChannel)?.Guild;
            Message = message;
            User = message.Author;
            Configuration = config;
        }
    }
}
