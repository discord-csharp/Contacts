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

                if (Global.Memos.ContainsKey(memoName))
                {
                    await EditAsync(memoName, memoResponse);
                    return;
                }
                Global.NewDataWritten = true;
                Global.Memos.AddOrUpdate(memoName, memoResponse, (key, value) => memoResponse);
                await ReplyAsync($"Added {memoName} to the dictionary of memos");
                await Task.Factory.StartNew(() => File.WriteAllText("memos.json", JsonConvert.SerializeObject(Global.Memos, Formatting.Indented)));
            }
            else
                await ReplyAsync("Couldn't add memo: Insufficient role");
        }

        [Command, Summary("Retrieves a memo")]
        public async Task GetAsync([Summary("The memo to retrieve")] string memo)
        {
            if (Global.Memos.TryGetValue(memo, out string result))
                await ReplyAsync($"{memo}: {result}");
            else
                await ReplyAsync($"Couldn't find {memo}");
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
                if (Global.Memos.TryRemove(memoName, out string memoNameOutput))
                {
                    Global.NewDataWritten = true;
                    await ReplyAsync($"Removed {memoName} from the memo dictionary");
                }
                else
                    await ReplyAsync($"Couldn't find {memoName} in the dictionary");
            }
            else
                await ReplyAsync("Couldn't remove memo: Insufficient role");
        }

        [Command, Summary("Lists all the existing memos")]
        public async Task ListAsync()
        {
            string reply = "Memos: ";
            foreach(var memo in Global.Memos.Keys)
            {
                if (memo == Global.Memos.Keys.First())
                    reply += memo;
                else
                    reply += $", {memo}";
            }
            await ReplyAsync(reply);
        }
    }
}
