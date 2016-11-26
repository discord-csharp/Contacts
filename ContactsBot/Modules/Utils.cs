using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactsBot.Modules
{
    public class Utils : ModuleBase
    {
        [Command("roleid")]
        public async Task RoleId(string roleName) // todo, make this util better
        {
            var role = Context.Guild.Roles.First(r => string.Equals(r.Name, roleName, StringComparison.OrdinalIgnoreCase));
            if (role == null)
                await ReplyAsync($"Couldn't find role with name: {roleName}");
            else
                await ReplyAsync($"\"{roleName}\" has ID {role.Id}");
        }
    }
}
