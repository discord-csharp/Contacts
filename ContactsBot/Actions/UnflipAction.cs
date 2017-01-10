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
    public class UnflipAction : ActionServices.ActionService
    {
        private Dictionary<IUser, int> _flips = new Dictionary<IUser, int>();
        
        public UnflipAction(IDependencyMap map) : base(map) { }

        public override async void Invoke(SocketMessage message)
        {
            await PerformUnflipAsync(message);
        }

        public async Task PerformUnflipAsync(SocketMessage message)
        {
            if (message.Content.EndsWith("(╯°□°）╯︵ ┻━┻"))
            {
                var flipCount = (_flips.ContainsKey(message.Author) ? _flips[message.Author]++ : _flips[message.Author] = 1);
                var replyMessage = message.Author.Mention + " ┬─┬﻿ ノ( ゜-゜ノ)";

                if (flipCount > 1)
                    replyMessage = $"**{replyMessage} CALM DOWN**";
                else if (flipCount > 2)
                    replyMessage = $"**{replyMessage} THIS IS NOT OKAY**";
                else if (flipCount > 3)
                    replyMessage = @"**┻━┻ ☆︵ /(.□. \) OW!**";

                await message.Channel.SendMessageAsync(replyMessage);
            }
            else
            {
                if (_flips.ContainsKey(message.Author))
                    _flips[message.Author] = 0;
            }
        }
    }
}
