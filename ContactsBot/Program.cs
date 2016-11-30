using Discord.API;
using Discord.API.Rest;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ContactsBot
{
    class Program
    {
        static void Main(string[] args) => new Program().RunBot().GetAwaiter().GetResult();

        DiscordSocketClient _client;
        DependencyMap _map;
        BotConfiguration _config;
        CommandHandler _handler;
        AntiAdvertisement _antiAdvertisement;

        public async Task RunBot()
        {
            if (!File.Exists("config.json"))
            {
                Console.Write("Please enter a bot token: ");
                string token = Console.ReadLine();
                _config = BotConfiguration.CreateBotConfigWithToken("config.json", token);
            }
            else
                _config = BotConfiguration.ProcessBotConfig("config.json");

            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                AudioMode = Discord.Audio.AudioMode.Disabled,
                LogLevel = Discord.LogSeverity.Debug
            });

            // Create the dependency map and inject the client and config into it
            _map = new DependencyMap();
            _map.Add(_client);
            _map.Add(_config);

            _handler = new CommandHandler();
            await _handler.Install(_map);

            _antiAdvertisement = new AntiAdvertisement();
            _antiAdvertisement.Install(_map);
            _antiAdvertisement.Enable();

            _client.Log += _client_Log; // console info

            await _client.LoginAsync(Discord.TokenType.Bot, _config.Token);
            await _client.ConnectAsync();

            await _client.SetGame("Helping you C#");

            await Task.Delay(-1);
        }

        private Task _client_Log(Discord.LogMessage arg)
        {
            switch (arg.Severity)
            {
                case Discord.LogSeverity.Error: Console.ForegroundColor = ConsoleColor.Red; break;
                case Discord.LogSeverity.Warning: Console.ForegroundColor = ConsoleColor.Yellow; break;
            }
            Console.WriteLine(arg.ToString());
            Console.ForegroundColor = ConsoleColor.Gray;
            return Task.CompletedTask;
        }
    }
}