using ContactsBot.Configuration;
using ContactsBot.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Humanizer;
using NLog;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ContactsBot.Modules
{
    [Name("Moderation Module")]
    public class ModerationModule : ModuleBase
    {
        static Logger ModeratorLogger { get; } = LogManager.GetCurrentClassLogger();
        private ConfigManager _config;
        private MuteService _muteService;

        public ModerationModule(ConfigManager config, MuteService muteService)
        {
            _config = config;
            _muteService = muteService;
        }

        [Command("mute")] 
        [Summary("Mutes a user for the specified amount of time")]
        [RequireContext(ContextType.Guild)] 
        [RequireUserPermission(ChannelPermission.MoveMembers)]
        public async Task MuteAsync([Summary("The user to mute")] IGuildUser user, [Summary("The TimeSpan to mute the user")] TimeSpan time)
        {
            var muteRole = guildUser.Guild.GetRole(_mutedRoleId.Value);
            if (muteRole == null)
            {
                await ReplyAsync("Couldn't mute user: The specified role doesn't exist");
            }
            else
                await guildUser.AddRolesAsync(muteRole);

            _botInterface.MutedUsers.TryAdd(user, DateTime.UtcNow + time);

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

        private void PollMuteUsers(object state)
        {
            if (_botInterface.MutedUsers.IsEmpty) return;

            var enumerate = _botInterface.MutedUsers.GetEnumerator();
            while (enumerate.MoveNext())
            {
                if (enumerate.Current.Value <= DateTime.UtcNow)
                    UnmuteInternalAsync(enumerate.Current.Key, true).Wait();
            }
        }

        private async Task UnmuteInternalAsync(IGuildUser user, bool isCallback)
        {
            if (!_mutedRoleId.HasValue) return;
            if (!isCallback && !Context.IsCorrectRole(StandardRoles))
            {
                ModeratorLogger.Error("Couldn't unmute user: Insufficient role");
                await ReplyAsync("Couldn't unmute user: Insufficient role");
                return;
            }
            var guildUser = user as SocketGuildUser;
            var muteRole = guildUser.Guild.GetRole(_mutedRoleId.Value);
            if (muteRole == null)
            {
                ModeratorLogger.Error("Couldn't unmute user: The specified role doesn't exist");
                await ReplyAsync("Couldn't unmute user: The specified role doesn't exist");
            }
            else
            {
                if (guildUser.RoleIds.Contains(_mutedRoleId.Value))
                    await guildUser.RemoveRolesAsync(muteRole);
                else
                    return;
            }
            
            await ReplyAsync($"Unmuted {user.Nickname ?? user.Username}");
            ModeratorLogger.Info($"Unmuted {user.Nickname ?? user.Username}");
            _botInterface.MutedUsers.TryRemove(user, out var outputTimer);
        }
    }

    [Group("message"), Name("Message Module")]
    public class MessagesPruneModule : ModuleBase
    {
        [Command("deleterange"), RequireBotPermission(GuildPermission.ManageMessages), RequireContext(ContextType.Guild)]
        public async Task DeleteAsync([Summary("The range of messages to delete")] int range)
        {
            var messageList = await Context.Channel.GetMessagesAsync(range).Flatten();
            await Context.Channel.DeleteMessagesAsync(messageList);

            await ReplyAsync($"Deleted the last {range} messages.");
        }

        [Command("deleterange"), RequireBotPermission(GuildPermission.ManageMessages), RequireContext(ContextType.Guild)]
        public async Task DeleteAsync([Summary("The message ID to start deleting at")] ulong startMessage, [Summary("The last message ID to delete")] ulong endMessage)
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

        [Command("wipe"), RequireBotPermission(GuildPermission.ManageMessages), RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task WipeAsync(IUser user, int count, int range)
        {
            var messageList = (await Context.Channel.GetMessagesAsync(range).Flatten()).ToList();
            var userMessage = messageList.Where(message => message.Author == user).Take(count);
            await Context.Channel.DeleteMessagesAsync(userMessage);
            await ReplyAsync($"Wiped {userMessage.Count()} messages from {user}");
        }
    }
}
