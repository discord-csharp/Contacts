using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Linq;
using Discord.Commands;
using Discord.WebSocket;

namespace ContactsBot
{
    public class CommandHandler
    {
        private CommandService _commands;
        private DiscordSocketClient _client;
        private BotConfiguration _config;
        IDependencyMap _map;

        public async Task Install(IDependencyMap map)
        {
            _map = map;
            _client = _map.Get<DiscordSocketClient>();
            _config = _map.Get<BotConfiguration>();
            _commands = new CommandService();
            _map.Add(_commands);
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
            _client.MessageReceived += HandleCommand;
        }

        private async Task HandleCommand(SocketMessage msg)
        {
            var message = msg as SocketUserMessage;
            if (message == null) return;

            int argPos = 0;

            if (message.HasCharPrefix(_config.PrefixCharacter, ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                var context = new CommandContext(_client, message);

                // Provide the dependency map when executing commands
                var result = await _commands.ExecuteAsync(context, argPos, _map);
                if (!result.IsSuccess && !(result is ParseResult))
                {
                    if (result is ExecuteResult)
                        await message.Channel.SendMessageAsync("```" + ((ExecuteResult)result).Exception.ToString() + "```");
                    else
                        await message.Channel.SendMessageAsync(result.ErrorReason);
                }
            }
        }
    }
}
