using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace ContactsBot
{
    public class Moderation : ModuleBase
    {
        [Command("mute"), Summary("Mutes a user for the specified amount of time")]
        public async Task Mute(SocketUser user, int time)
        {

        }
    }
}
