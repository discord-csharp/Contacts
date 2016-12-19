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
    public class UnflipAction : IMessageAction
    {
        private Dictionary<ulong, int> _flips = new Dictionary<ulong, int>();

        private DiscordSocketClient _client;

        public bool IsEnabled { get; private set; }

        public void Disable()
        {
            _client.MessageReceived -= PerformUnflipAsync;
            IsEnabled = false;
        }

        public void Enable()
        {
            _client.MessageReceived += PerformUnflipAsync;
            IsEnabled = true;
        }

        public void InstallAsync(IDependencyMap map)
        {
            _client = map.Get<DiscordSocketClient>();
        }

        public async Task PerformUnflipAsync(SocketMessage message)
        {
            if (message.Content.EndsWith("(╯°□°）╯︵ ┻━┻"))
            {
                var flipCount = (_flips.ContainsKey(message.Author.Id) ? _flips[message.Author.Id]++ : _flips[message.Author.Id] = 1);
                var replyMessage = message.Author.Mention + " ┬─┬﻿ ノ( ゜-゜ノ)";
                if (flipCount > 1)
                    if (flipCount == 2)
                        replyMessage = $"**{replyMessage} CALM DOWN**";
                    else if (flipCount == 3)
                        replyMessage = $"**{replyMessage} THIS IS NOT OKAY**";
                    else
                        replyMessage = @"**┻━┻ ☆︵ /(.□. \) OW!**";

                await message.Channel.SendMessageAsync(replyMessage);
            }
            else
            {
                if (_flips.ContainsKey(message.Author.Id))
                    _flips[message.Author.Id] = 0;
            }
        }
    }
}
