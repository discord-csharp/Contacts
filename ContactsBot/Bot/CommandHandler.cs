using System.Threading.Tasks;
using System.Reflection;
using System.Linq;
using Discord.Commands;
using Discord.WebSocket;
using ContactsBot.Configuration;
using NLog;

namespace ContactsBot
{
    public class CommandHandler : IMessageAction
    {
        private CommandService _commands;
        private DiscordSocketClient _client;
        private BotConfiguration _config;
        IDependencyMap _map;
        public bool IsEnabled { get; private set; } = true;
        static Logger CommandLogger { get; set; }
        static CommandHandler()
        {
            CommandLogger = LogManager.GetCurrentClassLogger();
        }
        public void Install(IDependencyMap map)
        {
            _map = map;
            _client = _map.Get<DiscordSocketClient>();
#if DEV
            _config = _map.Get<ConfigManager>().GetConfigAsync<BotConfiguration>(name: "dev").GetAwaiter().GetResult();
#else
            _config = _map.Get<ConfigManager>().GetConfigAsync<BotConfiguration>().GetAwaiter().GetResult();
#endif
            _commands = new CommandService();
            _map.Add(_commands);
            _commands.AddModulesAsync(Assembly.GetEntryAssembly()).Wait();
            _commands.Commands.AsParallel().ForAll(I => CommandLogger.Debug("Loaded command: {0} - {1}", I.Name, I.RunMode.ToString()));
        }

        public void Enable()
        {
            _client.MessageReceived += HandleCommandAsync;
            IsEnabled = true;
        }

        public void Disable()
        {
            _client.MessageReceived -= HandleCommandAsync;
            IsEnabled = false;
        }

        private async Task HandleCommandAsync(SocketMessage msg)
        {
            var message = msg as SocketUserMessage;
            if (message == null) return;

            int argPos = 0;
#if DEV
            if (msg.Channel.Name != (_config.FilterChannel ?? msg.Channel.Name)) return;
#endif
            if (message.HasCharPrefix(_config.PrefixCharacter, ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                var context = new CommandContext(_client, message);

                // Provide the dependency map when executing commands
                var result = await _commands.ExecuteAsync(context, argPos, _map);
                if (!result.IsSuccess)
                {
                    if ((result is SearchResult))
                        return;
                    if (result is ExecuteResult)
                        CommandLogger.Error("```" + ((ExecuteResult)result).Exception.ToString() + "```");
                    else
                        CommandLogger.Error(result.ErrorReason);
                }
                else
                    CommandLogger.Info($"\"{message.Author.Username}\" ran the following command: {message.Content}");
            }
        }
    }
}
