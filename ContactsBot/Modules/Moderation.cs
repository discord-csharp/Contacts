using Discord;
using Discord.API.Rest;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ContactsBot
{
    public class Moderation : ModuleBase
    {
        ulong mutedRoleId = 0;
        List<Timer> _usersMuted = new List<Timer>();

        [Command("mute"), Summary("Mutes a user for the specified amount of time")]
        public async Task Mute(IUser user, uint time = 30)
        {
            if (!IsCorrectRole(new[] { "Founders", "Moderators" }))
            {
                await ReplyAsync("Couldn't mute user: Insufficient role");
                return;
            }
            var guildUser = user as SocketGuildUser;
            if (guildUser == null) return;
            
            List<ulong> userRoles = guildUser.RoleIds.ToList();
            userRoles.Add(mutedRoleId);

            await guildUser.ModifyAsync(mod => new ModifyGuildMemberParams
            {
                RoleIds = userRoles.ToArray()
            });
            _usersMuted.Add(new Timer(TimerCallback, user, TimeSpan.FromMinutes(time).Minutes, -1));
            
            await ReplyAsync($"Muted {guildUser.Nickname ?? guildUser.Username} for {time} minutes");
        }

        public async void TimerCallback(object user)
        {
            var guildUser = user as SocketGuildUser;
            List<ulong> userRoles = guildUser.RoleIds.ToList();
            userRoles.Remove(mutedRoleId);

            await guildUser.ModifyAsync(mod => new ModifyGuildMemberParams
            {
                RoleIds = userRoles.ToArray()
            });
        }

        [Command("unmute"), Summary("Unmutes a user")]
        public async Task Unmute(IUser user)
        {
            // todo unmute user
        }

        public bool IsCorrectRole(string[] roleNames)
        {
            var roles = Context.Guild.Roles;
            var guildUser = Context.User as SocketGuildUser;
            if (guildUser == null || roles == null)
                return false;

            var rolesFromNames = roles.Where(r => roleNames.Any(n => r.Name == n));
            return guildUser.RoleIds.Any(id => rolesFromNames.Any(r => r.Id == id));
        }
    }
}
