using Discord;
using Humanizer;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ContactsBot.Modules
{
    [Name("Moderation Module")]
    public class Moderation : ModuleBase
    {
        const ulong _mutedRoleId = 251734975727009793;
        internal static string[] StandardRoles = new[] { "Founders", "Moderators", "Regulars", "Bot" };

        [Command("mute"), Summary("Mutes a user for the specified amount of time")]
        public async Task MuteAsync([Summary("The user to mute")] IGuildUser user, [Summary("The TimeSpan to mute the user")] TimeSpan time)
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

            Timer timer = new Timer(TimerCallbackAsync, user, (int)time.TotalMilliseconds, -1);
            Global.MutedUsers.TryAdd(user, timer);

            await ReplyAsync($"Muted {guildUser.Nickname ?? guildUser.Username} for {time.Humanize(3)}");
        }

        public async void TimerCallbackAsync(object user)
        {
            await UnmuteInternalAsync(user as IGuildUser, true);
        }

        [Command("unmute"), Summary("Unmutes a user")]
        public async Task UnmuteAsync([Summary("The user to unmute")] IGuildUser user)
        {
            await UnmuteInternalAsync(user, false);
        }

        public async Task UnmuteInternalAsync(IGuildUser user, bool isCallback)
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
            Global.MutedUsers.TryRemove(user, out var outputTimer);
        }
    }

    [Group("message"), Name("Message Module")]
    public class Messages : ModuleBase
    {
        [Command("deleterange"), RequireBotPermission(GuildPermission.ManageMessages), RequireContext(ContextType.Guild)]
        public async Task DeleteAsync([Summary("The range of messages to delete")] int range)
        {
            if (Context.IsCorrectRole(Moderation.StandardRoles))
            {
                var messageList = await Context.Channel.GetMessagesAsync(range).Flatten();
                await Context.Channel.DeleteMessagesAsync(messageList);

                await ReplyAsync($"Deleted the last {range} messages.");
                Global.IgnoreCount += range;
            }
            else
                await ReplyAsync("Couldn't delete messages: Insufficient role");
        }

        [Command("deleterange"), RequireBotPermission(GuildPermission.ManageMessages), RequireContext(ContextType.Guild)]
        public async Task DeleteAsync([Summary("The message ID to start deleting at")] ulong startMessage, [Summary("The last message ID to delete")] ulong endMessage)
        {
            if (Context.IsCorrectRole(Moderation.StandardRoles))
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
                Global.IgnoreCount += Math.Abs(endIndex - startIndex) + 1;
            }
            else
                await ReplyAsync("Couldn't delete messages: Insufficient role");
        }

        [Command("wipe"), RequireBotPermission(GuildPermission.ManageMessages), RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task Wipe(IUser user, int count, int range)
        {
            var messageList = (await Context.Channel.GetMessagesAsync(range).Flatten()).ToList();
            var userMessage = messageList.Where(message => message.Author == user).Take(count);
            await Context.Channel.DeleteMessagesAsync(userMessage);
            await ReplyAsync($"Wiped {userMessage.Count()} messages from {user}");
            Global.IgnoreCount += userMessage.Count();
        }
    }

    public static class ModerationExtensions
    {
        public static bool IsCorrectRole(this CommandContext Context, params string[] roleNames)
        {
            return (Context.User as IGuildUser).IsCorrectRole(Context.Guild, roleNames); // fucking perfect
        }

        public static bool IsCorrectRole(this IGuildUser guildUser, IGuild guild, params string[] roleNames)
        {
            var roles = guild.Roles;
            if (guildUser == null || roles == null)
                return false;

            var rolesFromNames = roles.Where(r => roleNames.Any(n => r.Name == n));
            return guildUser.RoleIds.Any(id => rolesFromNames.Any(r => r.Id == id));
        }
    }
}
