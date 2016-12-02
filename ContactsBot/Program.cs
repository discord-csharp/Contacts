using ContactsBot.Modules;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ContactsBot
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Directory.CreateDirectory("logs");
                _file = new FileStream(Path.Combine("logs", $"contacts-log-{DateTime.UtcNow.Ticks}-{Environment.TickCount}.txt"), FileMode.Create, FileAccess.Write);
                _logFile = new StreamWriter(_file) { AutoFlush = true };
                new Program().RunBot().GetAwaiter().GetResult();
            }
            catch(Exception e)
            {
                _logFile.WriteLine();
                _logFile.WriteLine(e.Source);
                _logFile.WriteLine(e.Message);
                _logFile.WriteLine(e.StackTrace);
            }
            finally
            {
                _logFile.Flush();
                _file.Flush();
                _logFile.Dispose();
                _file.Dispose();
            }
        }

        DiscordSocketClient _client;
        DependencyMap _map;
        BotConfiguration _config;
        CommandHandler _handler;
        static StreamWriter _logFile;
        static FileStream _file;
        ISocketMessageChannel _logChannel;

        public async Task RunBot()
        {
            if (!File.Exists("config.json"))
            {
                Console.Write("Please enter a bot token: ");
                string token = Console.ReadLine();
                _config = BotConfiguration.CreateBotConfigWithToken("config.json", token);
            }
            else
            {
                _config = BotConfiguration.ProcessBotConfig("config.json");
            }
#if DEV
            if (string.IsNullOrWhiteSpace(_config.DevToken))
            {
                Console.Write("Please enter a dev token: ");
                string token = Console.ReadLine();
                _config.DevToken = token;
                Console.WriteLine("Please enter a dev channel name (commands sent to the bot will be restricted to this channel): ");
                string channel = Console.ReadLine();
                _config.DevChannel = channel;
                _config.SaveBotConfig("config.json");
            }
#endif

            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                AudioMode = Discord.Audio.AudioMode.Disabled,
                LogLevel = LogSeverity.Debug
            });

            // Create the dependency map and inject the client and config into it
            _map = new DependencyMap();
            _map.Add(_client);
            _map.Add(_config);

            _handler = new CommandHandler();
            await _handler.Install(_map);

            AddAction<AntiAdvertisement>(_map);
            AddAction<Unflip>(_map);
            
            _client.Log += _client_Log; // console info
            _client.Log += FileLog; // file logs

            // handle logging to channel
            _client.UserJoined += ChannelLog_UserJoin;
            _client.UserLeft += ChannelLog_UserLeave;

#if DEV
            await _client.LoginAsync(TokenType.Bot, _config.DevToken);
#else
            await _client.LoginAsync(Discord.TokenType.Bot, _config.Token);
#endif
            await _client.ConnectAsync();

            await _client.SetGame("Helping you C#");

            await Task.Delay(-1);
        }

        private async Task ChannelLog_UserLeave(SocketGuildUser arg)
        {
            CheckLogChannel();
            await _logChannel.SendMessageAsync($"User left: Bye {arg.Username}!");
        }

        private void CheckLogChannel()
        {
            if (_logChannel == null) _logChannel = _client.GetGuild(143867839282020352).GetChannel(253967815671808001) as ISocketMessageChannel; // hack: someone make this better please
        }

        private async Task ChannelLog_UserJoin(SocketGuildUser arg)
        {
            CheckLogChannel();
            await _logChannel.SendMessageAsync($"User joined: Welcome {arg.Username} to the server!");
        }

        private Task _client_Log(LogMessage arg)
        {
            switch (arg.Severity)
            {
                case LogSeverity.Critical:
                case LogSeverity.Error: Console.ForegroundColor = ConsoleColor.Red; break;
                case LogSeverity.Warning: Console.ForegroundColor = ConsoleColor.Yellow; break;
                case LogSeverity.Info: Console.ForegroundColor = ConsoleColor.White; break;
                case LogSeverity.Verbose:
                case LogSeverity.Debug: Console.ForegroundColor = ConsoleColor.Cyan; break;
            }
            Console.WriteLine(arg.ToString());
            Console.ForegroundColor = ConsoleColor.Gray;
            return Task.CompletedTask;
        }

        private Task FileLog(LogMessage arg)
        {
            _logFile.WriteLine(arg.ToString());
            return Task.CompletedTask;
        }
        
        private void AddAction<T>(IDependencyMap map, bool autoEnable = true) where T : IMessageAction, new()
        {
            T action = new T();
            action.Install(map);
            if (autoEnable) action.Enable();
            Global.MessageActions.Add(action.GetType().Name, action);
        }
    }
}