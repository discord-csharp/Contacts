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
    public class ModerationModule : ModuleBase<ServerCommandContext>
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
            if(Context.Configuration.EnableMuteRole && Context.Configuration.MuteRole != 0)
            {
                _muteService.Mute(user, time, user.Guild.GetRole(Context.Configuration.MuteRole));
            }
            await ReplyAsync($"Muted {user.Nickname ?? user.Username} for {time.Humanize(3)}");
        }

        [Command("unmute"), Summary("Unmutes a user")]
        public async Task UnmuteAsync([Summary("The user to unmute")] IGuildUser user)
        {
            
        }
    }

    [Group("message"), Name("Message Module")]
    public class MessagesPruneModule : ModuleBase<ServerCommandContext>
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
