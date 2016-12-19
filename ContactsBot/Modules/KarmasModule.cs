using ContactsBot.Configuration;
using ContactsBot.Data;
using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using NLog;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactsBot.Modules
{
    [Group("karma"), Summary("Lets users store text for later use")]
    public class KarmasModule : ModuleBase
    {
        private ConfigManager _config;
        private static Logger KarmasModuleLogger { get; } = LogManager.GetCurrentClassLogger();
        public KarmasModule(ConfigManager config)
        {
            _config = config;
        }

        [Command("+1"), Summary("Give someone a +1 for solving your problem")]
        public async Task PlusOneAsync([Summary("the username string to recieve your thanks")] IGuildUser user)
        {
            if (user == null) return;

            try
            {
                using (var context = new ContactsBotDbContext())
                {
                    var databaseUserID = (long)user.Id;
                    var item = context.Karmas.FirstOrDefault(I => I.UserID == databaseUserID);
                    if (item != null)
                    {
                        item.KarmaCount++;
                        context.Karmas.Update(item);
                    }
                    else
                        await context.Karmas.AddAsync(new Karma() { UserID = (long)user.Id, KarmaCount = 1 });
                    await context.SaveChangesAsync();
                    await ReplyAsync($"Thanks {user.Username}");
                }
            }
            catch (Exception ex)
            {
                KarmasModuleLogger.Error(ex, ex.Message);
            }
        }

        [Command("top5"), Summary("Get the top 5 karma users")]
        public async Task TopFiveAsync()
        {
            try
            {
                using (var context = new ContactsBotDbContext())
                {
                    var output = new StringBuilder();
                    output.AppendLine("The Top 5 Karma Users:");

                    var Index = 1;
                    foreach (var item in context.Karmas.OrderByDescending(I => I.KarmaCount).Take(5))
                    {
                        var user = await Context.Guild.GetUserAsync((ulong)item.UserID);
                        output.AppendFormat("**{0})** {1} - {2} Karma\n", Index++, user.Username, item.KarmaCount);
                    }

                    await ReplyAsync(output.ToString());
                }
            }
            catch (Exception ex)
            {
                KarmasModuleLogger.Error(ex, ex.Message);
            }
        }
    }
}
