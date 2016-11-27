using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactsBot.Modules
{
    [Group("info"), Name("Info Module")]
    public class Info : ModuleBase
    {
        private CommandService _commands;

        [Command("help"), Summary("Displays this help message")]
        public async Task Help()
        {
            StringBuilder response = new StringBuilder();
            response.AppendLine("**Contacts Commands:**");
            response.AppendLine();
            foreach (var m in _commands.Modules)
            {
                if (m.Commands.Count() < 0) continue;
                response.AppendLine($"__{m.Name}__");
                response.AppendLine();
                foreach (var c in m.Commands)
                {
                    response.Append($"{c.Aliases.Aggregate((b, a) => b + " | " + a)} ");
                    foreach (var p in c.Parameters)
                    {
                        if (p.IsOptional)
                            response.Append($"[{p.Name}] ");
                        else
                            response.Append($"<{p.Name}> ");
                    }
                    response.AppendLine($" - {c.Summary}");
                    foreach (var p in c.Parameters)
                    {
                        string optional = p.IsOptional ? "(Optional)" : null;
                        response.AppendLine($"\t{p.Name} - {p.Summary} {optional}");
                    }
                    response.AppendLine();
                }
            }

            var replyChannel = await Context.User.GetDMChannelAsync() ?? await Context.User.CreateDMChannelAsync();
            await replyChannel.SendMessageAsync(response.ToString());
            await ReplyAsync("Check your DM's. I sent you a message.");
        }

        // Upon creation of this module, the command service will be loaded from the dependency map
        public Info(CommandService cs)
        {
            _commands = cs;
        }
    }
}
