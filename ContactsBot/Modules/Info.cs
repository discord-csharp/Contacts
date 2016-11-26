using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ContactsBot.Modules
{
    [Group("info")]
    public class Info : ModuleBase
    {
        [Command("help")]
        public async Task Help()
        {
            // todo: help command
        }
    }
}
