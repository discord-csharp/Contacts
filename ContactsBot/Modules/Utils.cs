using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using System.Linq;
using System.Text;
using Discord.WebSocket;
using System.Globalization;

namespace ContactsBot.Modules
{
    [Name("Utils Module")]
    public class Utils : ModuleBase
    {
        [Command("roleinfo"), Summary("Gets information about the specified role")]
        public async Task RoleInfoAsync([Summary("The role to find")]IRole role) // todo, make this util better
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

        [Command("whois"), Summary("Gets info about the specified user")]
        public async Task WhoIsAsync([Summary("The user to display info about")] IGuildUser user)
        {
            EmbedBuilder embed = new EmbedBuilder
            {
                Title = user.Username,
                ThumbnailUrl = user.AvatarUrl
            };
            if(user.IsBot)
                embed.AddField(field =>
                {
                    field.Name = "Bot user?";
                    field.Value = "True";
                });
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = "Id";
                field.Value = user.Id.ToString();
            });
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = "Status";
                field.Value = user.Status.ToString();
            });
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = "Joined at";
                field.Value = user.JoinedAt.HasValue ? user.JoinedAt.Value.ToString(CultureInfo.InvariantCulture.DateTimeFormat.ShortDatePattern) : string.Empty;
            });
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = "Created at";
                field.Value = user.CreatedAt.ToString(CultureInfo.InvariantCulture.DateTimeFormat.ShortDatePattern);
            });
            if(user.Nickname != null)
                embed.AddField(field =>
                {
                    field.Name = "Nickname";
                    field.Value = user.Nickname;
                });
            if(user.Game.HasValue)
                embed.AddField(field =>
                {
                    field.Name = "Playing";
                    field.Value = user.Game.Value.Name;
                });
            if (user.RoleIds.Last() != user.Guild.EveryoneRole.Id)
            {
                IRole role = null;
                foreach (ulong roleId in user.RoleIds)
                {
                    IRole currentRole = user.Guild.GetRole(roleId);
                    if (role == null || (role.Position < currentRole.Position && currentRole.IsHoisted))
                        role = currentRole;
                }
                embed.Color = role?.Color;
            }
            await ReplyAsync(string.Empty, embed: embed);
        }
    }

    [Name("Message Actions module"), Group("actions")]
    public class MessageActionModifiers : ModuleBase
    {
        [Command("list"), Summary("Lists actions installed for this bot")]
        public async Task ListActionsAsync()
        {
            StringBuilder reply = new StringBuilder();
            reply.Append("Actions:");
            foreach(var action in Global.MessageActions)
            {
                reply.AppendLine();
                var replyEnabled = action.Value.IsEnabled ? "Enabled" : "Disabled";
                reply.Append($"{action.Key}: {replyEnabled}");
            }
            await ReplyAsync(reply.ToString());
        }

        [Command("enable"), Summary("Enables a disabled action")]
        public async Task EnableActionAsync([Summary("The action to enable")] string actionName)
        {
            if(!Context.IsCorrectRole(Moderation.StandardRoles))
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

        [Command("disable"), Summary("Disables an enabled action")]
        public async Task DisableActionAsync([Summary("The action to disable")] string actionName)
        {
            if (!Context.IsCorrectRole(Moderation.StandardRoles))
            {
                await ReplyAsync("Couldn't disable message action: Insufficient role");
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
