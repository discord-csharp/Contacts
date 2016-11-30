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
    public class Unflip : IMessageAction
    {
        private Dictionary<ulong, int> _flips = new Dictionary<ulong, int>();

        private DiscordSocketClient _client;

        public bool IsEnabled { get; private set; }

        public void Disable()
        {
            _client.MessageReceived -= PerformUnflip;
            IsEnabled = false;
        }

        public void Enable()
        {
            _client.MessageReceived += PerformUnflip;
            IsEnabled = true;
        }

        public void Install(IDependencyMap map)
        {
            _client = map.Get<DiscordSocketClient>();
        }

        public async Task PerformUnflip(SocketMessage message)
        {
            ulong uid = message.Author.Id;

            if (message.Content.EndsWith("(╯°□°）╯︵ ┻━┻"))
            {
                int flipCount = (!_flips.ContainsKey(uid) ? _flips[uid] = 1 : _flips[uid]++);
                string replyMessage = message.Author.Mention + " ┬─┬﻿ ノ( ゜-゜ノ)";

                if (flipCount > 1)
                {
                    replyMessage = $"**{replyMessage} CALM DOWN**";
                }
                
                if (flipCount > 2)
                {
                    replyMessage = $"**{replyMessage} THIS IS NOT OKAY**";
                }

                await message.Channel.SendMessageAsync(replyMessage);
            }
            else
            {
                if(_flips.ContainsKey(uid))
                    _flips[uid] = 0;
            }
        }
    }
}
