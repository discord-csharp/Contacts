using Discord;
using Humanizer;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ContactsBot
{
    [Name("Moderation Module")]
    public class Moderation : ModuleBase
    {
        const ulong _mutedRoleId = 251734975727009793;
        internal static string[] StandardRoles = new[] { "Founders", "Moderators", "Regulars" };

        [Command("mute"), Summary("Mutes a user for the specified amount of time")]
        public async Task Mute([Summary("The user to mute")] IGuildUser user, [Summary("The time in minutes to mute the user")] TimeSpan time)
        {
            var guildUser = user as SocketGuildUser;
            if (guildUser == null) return;

            if (!Context.IsCorrectRole(StandardRoles))
             {
                await ReplyAsync("Couldn't mute user: Insufficient role");
                return;
            }
            
            var muteRole = guildUser.Guild.Roles.FirstOrDefault(r => r.Id == _mutedRoleId);
            if (muteRole == null)
            {
                await ReplyAsync("Couldn't mute user: The specified role doesn't exist");
            }
            else
                await guildUser.AddRolesAsync(muteRole);

            Timer timer = new Timer(TimerCallback, user, (int)time.TotalMilliseconds, -1);
            Global.MutedTimers.Add(timer);
            Global.MutedUsers.Add(user, timer);

            await ReplyAsync($"Muted {guildUser.Nickname ?? guildUser.Username} for {time.Humanize(3)}");
        }

        public async void TimerCallback(object user)
        {
            await UnmuteInternal(user as IGuildUser, true);
        }

        [Command("unmute"), Summary("Unmutes a user")]
        public async Task Unmute([Summary("The user to unmute")] IGuildUser user)
        {
            await UnmuteInternal(user, false);
        }

        public async Task UnmuteInternal(IGuildUser user, bool isCallback)
        {
            if (!isCallback && !Context.IsCorrectRole(StandardRoles))
            {
                await ReplyAsync("Couldn't unmute user: Insufficient role");
                return;
            }
            var guildUser = user as SocketGuildUser;
            var muteRole = guildUser.Guild.Roles.FirstOrDefault(r => r.Id == _mutedRoleId);
            if (muteRole == null)
            {
                await ReplyAsync("Couldn't unmute user: The specified role doesn't exist");
            }
            else
            {
                if (guildUser.RoleIds.Contains(_mutedRoleId))
                    await guildUser.RemoveRolesAsync(muteRole);
                else
                    return;
            }
            
            await ReplyAsync($"Unmuted {user.Nickname ?? user.Username}");

            Global.MutedTimers.Remove(Global.MutedUsers[user]);
            Global.MutedUsers.Remove(user);
        }
    }

    [Group("message"), Name("Message Module")]
    public class Messages : ModuleBase
    {
        [Command("deleterange")]
        public async Task Delete([Summary("The range of messages to delete")] int range)
        {
            if (Context.IsCorrectRole(Moderation.StandardRoles))
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
            if (Context.IsCorrectRole(Moderation.StandardRoles)) // todo: replace this array with something better
            {
                var messageList = (await Context.Channel.GetMessagesAsync(500).Flatten()).ToList();
                int startIndex = messageList.FindIndex(m => m.Id == startMessage);
                int endIndex = messageList.FindIndex(m => m.Id == endMessage);
                if(startIndex == -1 || endIndex == -1)
                {
                    await ReplyAsync("Couldn't delete messages: The start or end message ID couldn't be found");
                    return;
                }
                var messageRange = (startIndex > endIndex) ? messageList.GetRange(endIndex - 1, (startIndex - endIndex) + 1) : messageList.GetRange(startIndex, (endIndex - startIndex) + 1);
                await Context.Channel.DeleteMessagesAsync(messageRange);

                await ReplyAsync($"Deleted {Math.Abs(endIndex - startIndex) + 1} messages");
            }
            else
                await ReplyAsync("Couldn't delete messages: Insufficient role");
        }

        [Command("_invitedelete")]
        public async Task InviteDelete()
        {
            if (!Context.Message.Content.Contains("discord.gg/")) { return; }

            var authorAsGuildUser = Context.Message.Author as SocketGuildUser;
            if (authorAsGuildUser == null) { return; }

            if (!Context.IsCorrectRole(Moderation.StandardRoles))
            {
                await Context.Message.Channel.DeleteMessagesAsync(new[] { Context.Message });
                var dmChannel = await authorAsGuildUser.CreateDMChannelAsync();
                await dmChannel.SendMessageAsync("Your Discord invite link was removed. Please ask a staff member or regular to post it.");
            }
        }
    }

    public static class ModerationExtensions
    {
        public static bool IsCorrectRole(this CommandContext Context, params string[] roleNames)
        {
            var roles = Context.Guild.Roles;
            var guildUser = Context.User as IGuildUser;
            if (guildUser == null || roles == null)
                return false;

            var rolesFromNames = roles.Where(r => roleNames.Any(n => r.Name == n));
            return guildUser.RoleIds.Any(id => rolesFromNames.Any(r => r.Id == id));
        }
    }
}
