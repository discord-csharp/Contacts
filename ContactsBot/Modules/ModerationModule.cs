using Discord;
using Humanizer;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ContactsBot.Configuration;
using NLog;

namespace ContactsBot.Modules
{
    [Name("Moderation Module")]
    public class ModerationModule : ModuleBase
    {
        private static Logger ModeratorLogger { get; } = LogManager.GetCurrentClassLogger();
        private ConfigManager _config;
        private IBotInterface _botInterface;
        private ulong? _mutedRoleId = null;
        public ModerationModule(ConfigManager config, IBotInterface botInterface)
        {
            _config = config;
            _botInterface = botInterface;
#if DEV
            _mutedRoleId = config.GetConfigAsync<BotConfiguration>(name: "dev").GetAwaiter().GetResult().MuteRole;
#else
            _mutedRoleId = config.GetConfig<BotConfiguration>().GetAwaiter().GetResult().MuteRole;
#endif
        }

        internal static string[] StandardRoles = new[] { "Founders", "Administrator", "Moderators", "Regulars", "Bot" };

        [Command("mute"), Summary("Mutes a user for the specified amount of time")]
        public async Task MuteAsync([Summary("The user to mute")] IGuildUser user, [Summary("The TimeSpan to mute the user")] TimeSpan time)
        {
            if (!_mutedRoleId.HasValue) return;

            var guildUser = user as SocketGuildUser;
            if (guildUser == null) return;

            if (!Context.IsCorrectRole(StandardRoles))
             {
                await ReplyAsync("Couldn't mute user: Insufficient role");
                return;
            }
            
            var muteRole = guildUser.Guild.GetRole(_mutedRoleId.Value);
            if (muteRole == null)
            {
                await ReplyAsync("Couldn't mute user: The specified role doesn't exist");
            }
            else
                await guildUser.AddRolesAsync(muteRole);

            Timer timer = new Timer(TimerCallbackAsync, user, (int)time.TotalMilliseconds, -1);
            _botInterface.MutedUsers.TryAdd(user, timer);

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
            if (!_mutedRoleId.HasValue) return;
            if (!isCallback && !Context.IsCorrectRole(StandardRoles))
            {
                await ReplyAsync("Couldn't unmute user: Insufficient role");
                return;
            }
            var guildUser = user as SocketGuildUser;
            var muteRole = guildUser.Guild.GetRole(_mutedRoleId.Value);
            if (muteRole == null)
            {
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
            _botInterface.MutedUsers.TryRemove(user, out var outputTimer);
        }
    }

    [Group("message"), Name("Message Module")]
    public class MessagesModule : ModuleBase
    {
        IBotInterface _botInterface;

        public MessagesModule(IBotInterface botInterface)
        {
            _botInterface = botInterface;
        }

        [Command("deleterange"), RequireBotPermission(GuildPermission.ManageMessages), RequireContext(ContextType.Guild)]
        public async Task DeleteAsync([Summary("The range of messages to delete")] int range)
        {
            if (Context.IsCorrectRole(ModerationModule.StandardRoles))
            {
                var messageList = await Context.Channel.GetMessagesAsync(range).Flatten();
                await Context.Channel.DeleteMessagesAsync(messageList);

                await ReplyAsync($"Deleted the last {range} messages.");
                _botInterface.IgnoreCount += range;
            }
            else
                await ReplyAsync("Couldn't delete messages: Insufficient role");
        }

        [Command("deleterange"), RequireBotPermission(GuildPermission.ManageMessages), RequireContext(ContextType.Guild)]
        public async Task DeleteAsync([Summary("The message ID to start deleting at")] ulong startMessage, [Summary("The last message ID to delete")] ulong endMessage)
        {
            if (Context.IsCorrectRole(ModerationModule.StandardRoles))
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
                _botInterface.IgnoreCount += Math.Abs(endIndex - startIndex) + 1;
            }
            else
                await ReplyAsync("Couldn't delete messages: Insufficient role");
        }

        [Command("wipe"), RequireBotPermission(GuildPermission.ManageMessages), RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task WipeAsync(IUser user, int count, int range)
        {
            var messageList = (await Context.Channel.GetMessagesAsync(range).Flatten()).ToList();
            var userMessage = messageList.Where(message => message.Author == user).Take(count);
            await Context.Channel.DeleteMessagesAsync(userMessage);
            await ReplyAsync($"Wiped {userMessage.Count()} messages from {user}");
            _botInterface.IgnoreCount += userMessage.Count();
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
