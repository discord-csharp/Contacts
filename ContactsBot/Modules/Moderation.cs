using Discord;
using Discord.API.Rest;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace ContactsBot
{
    public class Moderation : ModuleBase
    {
        const ulong mutedRoleId = 251734975727009793;

        [Command("mute"), Summary("Mutes a user for the specified amount of time")]
        public async Task Mute([Summary("The user to mute")] IGuildUser user, [Summary("The time in minutes to mute the user")] double time = 30)
        {
            var guildUser = user as SocketGuildUser;
            if (guildUser == null) return;

            if (!IsCorrectRole(Context, new[] { "Founders", "Moderators", "Regulars" }))
             {
                await ReplyAsync("Couldn't mute user: Insufficient role");
                return;
            }

            TimeSpan timeToMute = TimeSpan.FromMinutes(time);

            var muteRole = guildUser.Guild.Roles.FirstOrDefault(r => r.Id == mutedRoleId);
            if (muteRole == null)
            {
                await ReplyAsync("Couldn't mute user: The specified role doesn't exist");
            }
            else
                await guildUser.AddRolesAsync(muteRole);

            Timer timer = new Timer(TimerCallback, user, (int)timeToMute.TotalMilliseconds, -1);
            Global.MutedTimers.Add(timer);
            Global.MutedUsers.Add(user, timer);

            await ReplyAsync($"Muted {guildUser.Nickname ?? guildUser.Username} for {time} minutes");
        }

        public async void TimerCallback(object user)
        {
            await Unmute(user as SocketGuildUser, true);
        }

        [Command("unmute"), Summary("Unmutes a user")]
        public async Task Unmute([Summary("The user to unmute")] IGuildUser user, bool isCallback = false)
        {
             if(!isCallback && !IsCorrectRole(Context, new[] { "Founders", "Moderators", "Regulars" }))
            {
                await ReplyAsync("Couldn't unmute user: Insufficient role");
                return;
            }
            var guildUser = user as SocketGuildUser;
            var muteRole = guildUser.Guild.Roles.FirstOrDefault(r => r.Id == mutedRoleId);
            if (muteRole == null)
            {
                await ReplyAsync("Couldn't unmute user: The specified role doesn't exist");
            }
            else
            {
                if (guildUser.RoleIds.Contains(mutedRoleId))
                    await guildUser.RemoveRolesAsync(muteRole);
                else
                    return;
            }

            Global.MutedTimers.Remove(Global.MutedUsers[user]);

            await ReplyAsync($"Unmuted {user.Nickname ?? user.Username}");
        }

        public static bool IsCorrectRole(CommandContext Context, string[] roleNames)
        {
            var roles = Context.Guild.Roles;
            var guildUser = Context.User as SocketGuildUser;
            if (guildUser == null || roles == null)
                return false;

            var rolesFromNames = roles.Where(r => roleNames.Any(n => r.Name == n));
            return guildUser.RoleIds.Any(id => rolesFromNames.Any(r => r.Id == id));
        }
    }

    [Group("message")]
    public class Messages : ModuleBase
    {
        [Command("deleterange")]
        public async Task Delete([Summary("The range of messages to delete")] int range)
        {
            if (Moderation.IsCorrectRole(Context, new[] { "Founders", "Moderators", "Regulars" }))
            {
                var messageList = await Context.Channel.GetMessagesAsync(range).Flatten();
                await Context.Channel.DeleteMessagesAsync(messageList);

                await ReplyAsync($"Deleted the last {range} messages.");
            }
            else
                await ReplyAsync("Couldn't delete messages: Insufficient role");
        }

        [Command("deleterange")]
        public async Task Delete([Summary("The most recent message ID to start deleting at")] ulong startMessage, [Summary("The last message ID to delete")] ulong endMessage)
        {
            if (Moderation.IsCorrectRole(Context, new[] { "Founders", "Moderators", "Regulars" })) // todo: replace this array with something better
            {
                var messageList = (await Context.Channel.GetMessagesAsync(500).Flatten()).ToList();
                int startIndex = messageList.IndexOf(messageList.FirstOrDefault(m => m.Id == startMessage));
                int endIndex = messageList.IndexOf(messageList.FirstOrDefault(m => m.Id == endMessage), startIndex);
                await Context.Channel.DeleteMessagesAsync(messageList.GetRange(startIndex + 1, endIndex - startIndex));

                await ReplyAsync($"Deleted {endIndex - startIndex} messages");
            }
            else
                await ReplyAsync("Couldn't delete message: Insufficient role");
        }
    }
}
