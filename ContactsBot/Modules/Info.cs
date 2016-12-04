using Discord.Commands;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactsBot.Modules
{
    [Group("info"), Name("Info Module")]
    public class Info : ModuleBase
    {
        private CommandService _commands;
        private BotConfiguration _config;

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

            await ReplyAsync(response.ToString());
        }

        [Command("motd"), Summary("Displays the current MOTD")]
        public async Task Motd()
        {
            await ReplyAsync(string.Format(_config.MessageOfTheDay, Context.User.Username));
        }

        // Upon creation of this module, the command service will be loaded from the dependency map
        public Info(CommandService cs, BotConfiguration config)
        {
            _commands = cs;
            _config = config;
        }
    }
}
