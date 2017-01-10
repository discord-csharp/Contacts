using System.Threading.Tasks;
using System.Reflection;
using System.Linq;
using Discord.Commands;
using Discord.WebSocket;
using ContactsBot.Configuration;
using NLog;
using Discord;
using System.Collections.Concurrent;

namespace ContactsBot
{
    public class CommandHandler : ActionServices.ActionService
    {
        private IDependencyMap _map;
        private CommandService _commands;
        private ServerConfiguration _config;
        private ConcurrentDictionary<IGuild, ServerConfiguration> _serverConfigs;
        static Logger CommandLogger { get; set; }

        public CommandHandler(IDependencyMap map) : base(map)
        {
            CommandLogger = LogManager.GetCurrentClassLogger();

            _map = map;
            _commands = new CommandService();
            map.Add(_commands);
            _serverConfigs = map.Get<ConcurrentDictionary<IGuild, ServerConfiguration>>();
            _commands.AddModulesAsync(Assembly.GetEntryAssembly()).Wait();
            _commands.Commands.AsParallel().ForAll(I => CommandLogger.Debug("Loaded command: {0} - {1}", I.Name, I.RunMode.ToString()));
        }

        public override void Enable()
        {
            Client.MessageReceived += HandleCommandAsync;
        }

        public override void Disable()
        {
            Client.MessageReceived -= HandleCommandAsync;
        }

        private async Task HandleCommandAsync(SocketMessage msg)
        {
            var message = msg as SocketUserMessage;
            if (message == null) return;
            if (!_serverConfigs.TryGetValue((message.Channel as IGuildChannel).Guild, out var config))
                return;

            int argPos = 0;

            if (message.HasStringPrefix(_config.PrefixString, ref argPos) || message.HasMentionPrefix(Client.CurrentUser, ref argPos))
            {
                var context = new ServerCommandContext(Client, message, config);

                // Provide the dependency map when executing commands
                var result = await _commands.ExecuteAsync(context, argPos, _map);
                if (!result.IsSuccess)
                {
                    if ((result is SearchResult))
                        return;
                    if (result is ExecuteResult exeResult)
                        CommandLogger.Error("```" + exeResult.Exception.ToString() + "```");
                    else
                        CommandLogger.Error(result.ErrorReason);
                }
                else
                    CommandLogger.Info($"\"{message.Author.Username}\" ran the following command: {message.Content}");
            }
        }
    }
}
