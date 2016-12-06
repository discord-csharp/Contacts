using ContactsBot.Data;
using Discord.Commands;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ContactsBot.Modules
{
    [Group("memos"), Summary("Lets users store text for later use")]
    public class Memos : ModuleBase
    {
        [Command("add"), Summary("Adds a memo to the memo dictionary")]
        public async Task AddAsync([Summary("The string to use to get the memo later")] string memoName, [Remainder, Summary("The string given back when this memo is called")] string memoResponse)
        {
            if (Context.IsCorrectRole(Moderation.StandardRoles))
            {
                if (memoName.StartsWith("add") || memoName.StartsWith("remove"))
                {
                    await ReplyAsync("The memo you submitted starts with \"add\" or \"remove\" and couldn't be added.");
                    return;
                }
                using (var context = new ContactsBotDbContext())
                {
                    var memo = context.Memos.FirstOrDefault(I => I.Key == memoName.ToLower());
                    if (!string.IsNullOrEmpty(memo.Key))
                    {
                        memo.Message = memoResponse;
                        context.Memos.Update(memo);
                        await context.SaveChangesAsync();
                        await ReplyAsync($"Updated {memoName} in the memos database");
                        return;
                    }
                    else
                    {
                        context.Memos.Add(new Memo() { CreatedBy = Context.User.Username, Key = memoName, Message = memoResponse });
                        await context.SaveChangesAsync();
                    }
                }
                await ReplyAsync($"Added {memoName} in the memos database");
            }
            else
                await ReplyAsync("Couldn't add memo: Insufficient role");
        }

        [Command, Summary("Retrieves a memo")]
        public async Task GetAsync([Summary("The memo to retrieve")] string memo)
        {
            using (var context = new ContactsBotDbContext())
            {
                var item = context.Memos.FirstOrDefault(I => I.Key == memo);
                if (!string.IsNullOrEmpty(item.Key))
                    await ReplyAsync($"{memo}: {item.Message} by {item.CreatedBy}");
                else
                    await ReplyAsync($"Couldn't find {memo}");
            }
        }

        [Command, Summary("Edits an already existing memo")]
        public async Task EditAsync([Summary("The already existing memo")] string name, [Summary("The new value the memo will use")] string newValue)
        {
            await RemoveAsync(name);
            await AddAsync(name, newValue);
        }

        [Command("remove"), Summary("Removes a memo from the memo dictionary")]
        public async Task RemoveAsync([Summary("The memo to remove")] string memoName)
        {
            if (Context.IsCorrectRole(Moderation.StandardRoles))
            {
                using (var context = new ContactsBotDbContext())
                {
                    var item = context.Memos.FirstOrDefault(I => I.Key == memoName);
                    if (!string.IsNullOrEmpty(item.Key))
                    {
                        context.Memos.Remove(item);
                        await context.SaveChangesAsync();
                        await ReplyAsync($"Removed {memoName} from the memo dictionary");
                    }
                    else
                        await ReplyAsync($"Couldn't find {memoName} in the dictionary");
                }
            }
            else
                await ReplyAsync("Couldn't remove memo: Insufficient role");
        }

        [Command, Summary("Lists all the existing memos")]
        public async Task ListAsync()
        {
            string reply = "Memos: ";
            bool first = true;
            using (var context = new ContactsBotDbContext())
                foreach (var memo in context.Memos)
                {
                    if (first)
                        reply += memo.Key;
                    else
                        reply += $", {memo}";
                    first = false;
                }
            await ReplyAsync(reply);
        }
    }
}
