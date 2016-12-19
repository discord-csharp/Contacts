using ContactsBot.Configuration;
using ContactsBot.Data;
using Discord.Commands;
using System.Linq;
using System.Threading.Tasks;

namespace ContactsBot.Modules
{
    [Group("memos"), Summary("Lets users store text for later use")]
    public class MemoModule : ModuleBase
    {
        private ConfigManager _config;

        public MemoModule(ConfigManager config)
        {
            _config = config;
        }

        [Command("add"), Summary("Adds a memo to the memo dictionary")]
        public async Task AddAsync([Summary("The string to use to get the memo later")] string memoName, [Remainder, Summary("The string given back when this memo is called")] string memoResponse)
        {
            memoName = memoName.ToLower();
            if (Context.IsCorrectRole(ModerationModule.StandardRoles))
            {
                if (memoName.StartsWith("add") || memoName.StartsWith("remove"))
                {
                    await ReplyAsync("The memo you submitted starts with \"add\" or \"remove\" and couldn't be added.");
                    return;
                }
                using (var context = new ContactsBotDbContext())
                {
                    var memo = context.Memos.FirstOrDefault(I => I.MemoName == memoName);
                    if (!string.IsNullOrEmpty(memo?.MemoName))
                    {
                        memo.Message = memoResponse;
                        context.Memos.Update(memo);
                        await context.SaveChangesAsync();
                        await ReplyAsync($"Updated {memoName} in the memos database");
                        return;
                    }

                    context.Memos.Add(new Memo() { UserID = (long)Context.User.Id, MemoName = memoName, Message = memoResponse });
                    await context.SaveChangesAsync();
                }
                await ReplyAsync($"Added {memoName} in the memos database");
            }
            else
                await ReplyAsync("Couldn't add memo: Insufficient role");
        }

        [Command, Summary("Retrieves a memo")]
        public async Task GetAsync([Summary("The memo to retrieve")] string memo)
        {
            memo = memo.ToLower();
            using (var context = new ContactsBotDbContext())
            {
                var item = context.Memos.FirstOrDefault(I => I.MemoName == memo);
                if (!string.IsNullOrEmpty(item.MemoName))
                {
                    var user = await Context.Guild.GetUserAsync((ulong)item.UserID);
                    await ReplyAsync("", false, new Discord.EmbedBuilder
                    {
                        Title = memo,
                        Description = item.Message,
                        Color = new Discord.Color(173, 255, 47),
                        Footer = new Discord.EmbedFooterBuilder
                        {
                            Text = $"Created by {user.Username}"
                        }
                    });
                }
                else
                    await ReplyAsync($"Couldn't find {memo}");
            }
        }

        [Command, Summary("Edits an already existing memo")]
        public async Task EditAsync([Summary("The already existing memo")] string memoName, [Summary("The new value the memo will use")] string newMessage)
        {
            if (Context.IsCorrectRole(ModerationModule.StandardRoles))
            {
                memoName = memoName.ToLower();
                if (memoName.StartsWith("add") || memoName.StartsWith("remove"))
                {
                    await ReplyAsync("The memo you submitted starts with \"add\" or \"remove\" and couldn't be added.");
                    return;
                }
                using (var context = new ContactsBotDbContext())
                {
                    var memo = context.Memos.FirstOrDefault(I => I.MemoName == memoName);
                    if (!string.IsNullOrEmpty(memo.MemoName))
                    {
                        memo.Message = newMessage;
                        context.Memos.Update(memo);
                        await ReplyAsync($"Updated {memoName} in the memos database");
                    }
                    else
                    {
                        context.Memos.Add(new Memo() { MemoName = memoName, Message = newMessage, UserID = (long)Context.User.Id });
                        await ReplyAsync($"Added {memoName} in the memos database");
                    }
                    await context.SaveChangesAsync();
                }
            }
            else
                await ReplyAsync("Couldn't update memo: Insufficient role");
        }

        [Command("remove"), Summary("Removes a memo from the memo dictionary")]
        public async Task RemoveAsync([Summary("The memo to remove")] string memoName)
        {
            if (Context.IsCorrectRole(ModerationModule.StandardRoles))
            {
                memoName = memoName.ToLower();
                using (var context = new ContactsBotDbContext())
                {
                    var item = context.Memos.FirstOrDefault(I => I.MemoName == memoName);
                    if (!string.IsNullOrEmpty(item.MemoName))
                    {
                        context.Memos.Remove(item);
                        await context.SaveChangesAsync();
                        await ReplyAsync($"Removed {memoName} from the memo database");
                    }
                    else
                        await ReplyAsync($"Couldn't find {memoName} in the database");
                }
            }
            else
                await ReplyAsync("Couldn't remove memo: Insufficient role");
        }

        [Command, Summary("Lists all the existing memos")]
        public async Task ListAsync()
        {
            var reply = "Memos: ";
            var first = true;
            using (var context = new ContactsBotDbContext())
                foreach (var memo in context.Memos)
                {
                    if (first)
                        reply += memo.MemoName;
                    else
                        reply += $", {memo}";
                    first = false;
                }
            await ReplyAsync(reply);
        }
    }
}
