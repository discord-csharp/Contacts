using ContactsBot.Data;
using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ContactsBot.Modules
{
    [Group("karma"), Summary("Lets users store text for later use")]
    public class Karmas : ModuleBase
    {
        [Command("+1"), Summary("Give someone a +1 for solving your problem")]
        public async Task PlusOneAsync([Summary("the username string to recieve your thanks")] IGuildUser user)
        {
            using (var context = new ContactsBotDbContext())
            {
                var item = context.Karmas.FirstOrDefault(I => I.Username == user.Username);
                if (!string.IsNullOrEmpty(item.Username))
                {
                    item.KarmaCount++;
                    context.Karmas.Update(item);
                    await context.SaveChangesAsync();
                }
                else
                {
                    await context.Karmas.AddAsync(new Karma() { Username = user.Username, KarmaCount = 1 });
                    await context.SaveChangesAsync();
                }
                await ReplyAsync($"Thanks {user.Username}");
            }
        }

        [Command("Top5"), Summary("Get the top 5 karma users")]
        public async Task TopFiveAsync()
        {
            using (var context = new ContactsBotDbContext())
            {
                var output = "The Top 5 Karma Users:\n";
                var users = context.Karmas.OrderByDescending(I => I.KarmaCount).Take(5).ToArray();

                for (var I = 0; I < users.Length; I++)
                    output = (I < users.Length - 1) ?
                        string.Format("{0}{1}. {2} - {3} karmas\n", output, I, users[I].Username, users[I].KarmaCount) :
                        string.Format("{0}{1}. {2} - {3} karmas", output, I, users[I].Username, users[I].KarmaCount);

                await ReplyAsync(output);
            }
        }
    }
}
