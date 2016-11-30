using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using System.Linq;
using System.Text;

namespace ContactsBot.Modules
{
    [Name("Utils Module")]
    public class Utils : ModuleBase
    {
        [Command("roleinfo"), Summary("Gets information about the specified role")]
        public async Task RoleInfo([Summary("The role to find")]IRole role) // todo, make this util better
        {
            EmbedBuilder embed = new EmbedBuilder
            {
                Color = role.Color,
                Title = role.Name
            };
            embed.AddField(field => { field.Name = "ID"; field.Value = role.Id.ToString(); });
            embed.AddField(field => { field.Name = "Discord managed"; field.Value = role.IsManaged.ToString(); });
            embed.AddField(field => { field.Name = "Mentionable"; field.Value = role.IsMentionable.ToString(); });
            embed.AddField(async (field) => { field.Name = "Number of users"; field.Value = (await Context.Guild.GetUsersAsync()).Count(u => u.RoleIds.Contains(role.Id)).ToString(); });
            await ReplyAsync(string.Empty, embed: embed);
        }
    }

    [Name("Message Actions module"), Group("actions")]
    public class MessageActionModifiers : ModuleBase
    {
        [Command("list")]
        public async Task ListActions()
        {
            StringBuilder reply = new StringBuilder();
            reply.Append("Action: Enabled");
            foreach(var action in Global.MessageActions)
            {
                reply.AppendLine();
                reply.Append($"{action.Key}: {action.Value.IsEnabled}");
            }
        }

        [Command("enable")]
        public async Task EnableAction(string actionName)
        {
            if(Context.IsCorrectRole("Founders", "Moderators"))
            {
                await ReplyAsync("Couldn't enable message action: Insufficient role");
                return;
            }
            Global.MessageActions.TryGetValue(actionName, out var action);
            if (action == null) await ReplyAsync("Couldn't find the specified action");
            if (action.IsEnabled) await ReplyAsync("The action is already enabled");
            else
            {
                action.Enable();
                await ReplyAsync($"Enabled {actionName}");
            }
        }

        [Command("disable")]
        public async Task DisableAction(string actionName)
        {
            if (Context.IsCorrectRole("Founders", "Moderators"))
            {
                await ReplyAsync("Couldn't enable message action: Insufficient role");
                return;
            }
            Global.MessageActions.TryGetValue(actionName, out var action);
            if (action == null) await ReplyAsync("Couldn't find the specified action");
            if (!action.IsEnabled) await ReplyAsync("The action is already disabled");
            else
            {
                action.Disable();
                await ReplyAsync($"Disabled {actionName}");
            }
        }
    }
}
