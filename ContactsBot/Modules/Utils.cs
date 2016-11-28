using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using System.Linq;

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
}
