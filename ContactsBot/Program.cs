using ContactsBot.Data;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ContactsBot
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!PostgreSQLGlobalConfig.InitializePostgreSQLGlobalConfig()) return;
            try
            {
                // todo: hook into application exiting to turn off bot
                if (!Directory.Exists("logs"))
                    Directory.CreateDirectory("logs");
                _file = new FileStream(Path.Combine("logs", $"contacts-log-{DateTime.UtcNow.Ticks}-{Environment.TickCount}.txt"), FileMode.Create, FileAccess.Write);
                _logFile = new StreamWriter(_file) { AutoFlush = true };
                new Program().RunBotAsync().GetAwaiter().GetResult();
            }
            catch (Exception e)
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
        ISocketMessageChannel LogChannel
        {
            get
            {
                if (_logChannel == null)
                    _logChannel = _client.GetGuild(143867839282020352).Channels.FirstOrDefault(c => c.Name == _config.LoggingChannel) as ISocketMessageChannel;
                return _logChannel;
            }
        }

        public async Task RunBotAsync()
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
                _config.SaveBotConfig("config.json");
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
                LogLevel = LogSeverity.Debug,
                MessageCacheSize = 2000
            });

            // Create the dependency map and inject the client and config into it
            _map = new DependencyMap();
            _map.Add(_client);
            _map.Add(_config);
            _map.Add(this);

            _handler = new CommandHandler();
            await _handler.InstallAsync(_map);

            AddAssemblyActions(_map);

            _client.Log += _client_LogAsync; // console info
            _client.Log += FileLog; // file logs

            // handle logging to channel
            _client.UserJoined += ChannelLog_UserJoinAsync;
            _client.UserLeft += ChannelLog_UserLeaveAsync;
            _client.UserBanned += ChannelLog_UserBannedAsync;
            _client.UserUnbanned += ChannelLog_UserUnbannedAsync;
            _client.MessageDeleted += ChannelLog_MessageDeletedAsync;

#if DEV
            await _client.LoginAsync(TokenType.Bot, _config.DevToken);
#else
            await _client.LoginAsync(Discord.TokenType.Bot, _config.Token);
#endif
            await _client.ConnectAsync();

            await _client.SetGame("Helping you C#");

            await Task.Delay(-1);
        }

        private async Task ChannelLog_MessageDeletedAsync(ulong arg1, Optional<SocketMessage> arg2)
        {
            if (Global.IgnoreCount == 0)
            {
                await LogChannel?.SendMessageAsync($"Message {arg1} was deleted.");
                if (arg2.IsSpecified)
                    await LogChannel?.SendMessageAsync($"The message was provided:\n{arg2.Value.Content}");
            }
            else Global.IgnoreCount--;
        }

        private async Task ChannelLog_UserUnbannedAsync(SocketUser arg1, SocketGuild arg2)
        {
            await LogChannel?.SendMessageAsync($"\"{arg1.Username}\" was unbanned from \"{arg2.Name}\"");
        }

        private async Task ChannelLog_UserBannedAsync(SocketUser arg1, SocketGuild arg2)
        {
            await LogChannel?.SendMessageAsync($"\"{arg1.Username}\" was banned from \"{arg2.Name}\"");
        }

        private async Task ChannelLog_UserLeaveAsync(SocketGuildUser arg)
        {
            await LogChannel?.SendMessageAsync($"User left: Bye \"{arg.Username}\"!");
        }

        private async Task ChannelLog_UserJoinAsync(SocketGuildUser arg)
        {
            await LogChannel?.SendMessageAsync($"User joined: Welcome \"{arg.Username}\" to the server!");
        }

        internal async Task ChannelLog_CommandLogAsync(string message)
        {
            await LogChannel?.SendMessageAsync(message);
        }

        private async Task _client_LogAsync(LogMessage arg)
        {
            switch (arg.Severity)
            {
                case LogSeverity.Critical:
                case LogSeverity.Error:
                    await ChannelLog_CommandLogAsync(arg.ToString());
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogSeverity.Warning: Console.ForegroundColor = ConsoleColor.Yellow; break;
                case LogSeverity.Info: Console.ForegroundColor = ConsoleColor.White; break;
                case LogSeverity.Verbose:
                case LogSeverity.Debug: Console.ForegroundColor = ConsoleColor.Cyan; break;
            }
            Console.WriteLine(arg.ToString());
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        private Task FileLog(LogMessage arg)
        {
            _logFile.WriteLine(arg.ToString());
            return Task.CompletedTask;
        }

        private void AddAction<T>(IDependencyMap map, bool autoEnable = true) where T : IMessageAction, new()
        {
            AddAction(typeof(T), map, autoEnable);
        }

        private void AddAction(Type handlerType, IDependencyMap map, bool autoEnable = true)
        {
            var handler = Activator.CreateInstance(handlerType) as IMessageAction;

            handler.Install(map);
            if (autoEnable) handler.Enable();
            Global.MessageActions.TryAdd(handler.GetType().Name, handler);
        }

        private void AddAssemblyActions(IDependencyMap map, bool autoEnable = true)
        {
            var allActions = Assembly.GetEntryAssembly().GetTypes().Where(d => d.GetInterfaces().Contains(typeof(IMessageAction)));

            foreach (var type in allActions)
            {
                AddAction(type, map, autoEnable);
            }
        }
    }
}