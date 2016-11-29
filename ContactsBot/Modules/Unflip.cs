using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactsBot.Modules
{
    public class Unflip : ModuleBase
    {
        private static Dictionary<ulong, int> Flips = new Dictionary<ulong, int>();

        [Command("_unflip")]
        public async Task PerformUnflip()
        {
            ulong uid = Context.User.Id;

            if (Context.Message.Content.EndsWith("(╯°□°）╯︵ ┻━┻"))
            {
                int flipCount = (!Flips.ContainsKey(uid) ? Flips[uid] = 1 : Flips[uid]++);
                string message = Context.User.Mention + " ┬─┬﻿ ノ( ゜-゜ノ)";

                if (flipCount > 1)
                {
                    message = $"**{message} CALM DOWN**";
                }
                
                if (flipCount > 2)
                {
                    message = $"**{message} THIS IS NOT OKAY**";
                }

                await Context.Channel.SendMessageAsync(message);
            }
            else
            {
                Flips[uid] = 0;
            }
        }
    }
}
