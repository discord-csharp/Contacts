using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using System.Linq;
using System.Globalization;
using NLog;

namespace ContactsBot.Modules
{
    [Name("Utils Module")]
    [Group("utils"), Summary("Utility Module for getting information on Discord")]
    public class UtilsModule : ModuleBase<ServerCommandContext>
    {
        private static Logger UtilsLogger { get; } = LogManager.GetCurrentClassLogger();

        [Command("roleinfo"), Summary("Gets information about the specified role")]
        public async Task RoleInfoAsync([Summary("The role to find")]IRole role) // TODO: make this util better
        {
            EmbedBuilder embed = new EmbedBuilder
            {
                Color = role.Color,
                Title = role.Name
            };
            embed.AddField(field => { field.Name = "ID"; field.Value = role.Id.ToString(); });
            embed.AddField(field => { field.Name = "Discord managed"; field.Value = role.IsManaged.ToString(); });
            embed.AddField(field => { field.Name = "Mentionable"; field.Value = role.IsMentionable.ToString(); });
            embed.AddField(async (field) => {
                field.Name = "Number of users";
                field.Value = (await Context.Guild.GetUsersAsync())
                               .Count(u => u.RoleIds.Contains(role.Id)).ToString();
            });

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
            if (user.IsBot)
                AddField(embed, "Is bot?", "True");
            AddField(embed, "ID", user.Id.ToString(), true);
            AddField(embed, "Status", user.Status.ToString(), true);
            AddField(embed, "Joined at", user.JoinedAt.HasValue ? user.JoinedAt.Value.ToString(CultureInfo.InvariantCulture.DateTimeFormat.ShortDatePattern + " " + CultureInfo.InvariantCulture.DateTimeFormat.LongTimePattern) : "Undetermined", true);
            AddField(embed, "Created at", user.CreatedAt.ToString(CultureInfo.InvariantCulture.DateTimeFormat.ShortDatePattern + " " + CultureInfo.InvariantCulture.DateTimeFormat.LongTimePattern), true);
            AddField(embed, "Nickname", user.Nickname);
            if(user.Game.HasValue)
                AddField(embed, "Game", $"Playing {user.Game.Value}");
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

        public static EmbedBuilder AddField(EmbedBuilder builder, string name, string value, bool inline = false)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(value))
                return builder;

            return builder.AddField(field => new EmbedFieldBuilder
            {
                IsInline = inline,
                Value = value,
                Name = name
            });
        }
    }
}
