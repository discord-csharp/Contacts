using ContactsBot.Configuration;
using ContactsBot.Utility;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using NLog;
using System.Reflection;
using System.Linq;
using System.Collections.Concurrent;
using System.Threading;
using ContactsBot.NLogTargets;
using NLog.Layouts;

namespace ContactsBot
{
    public class ContactsBot : IBotInterface
    {
        DiscordSocketClient _client;
        DependencyMap _map;
        BotConfiguration _config;
        ConfigManager _cfgMgr;
        internal static Logger BotLogger { get; set; }
        public ConcurrentDictionary<IGuildUser, Timer> MutedUsers { get; } = new ConcurrentDictionary<IGuildUser, Timer>();
        public ConcurrentDictionary<string, IMessageAction> MessageActions { get; } = new ConcurrentDictionary<string, IMessageAction>(StringComparer.OrdinalIgnoreCase);
        public long IgnoreCount { get; set; } = 0;
        private DiscordNLogTarget _discordNLogTarget;
        static ContactsBot()
        {
            BotLogger = LogManager.GetCurrentClassLogger();
        }

        public ContactsBot()
        {
            
        }

        public async Task RunBotAsync()
        {
            // Create the dependency map and inject the client and config into it
            _map = new DependencyMap();
            _cfgMgr = new ConfigManager("Configs");

            _map.Add(_cfgMgr);
            _map.Add(this);

#if DEV
            if (!_cfgMgr.ConfigExists<BotConfiguration>("dev"))
            {
                Console.Write("Please enter a dev token: ");
                string token = Console.ReadLine();
                Console.WriteLine("Please enter a dev channel name (commands sent to the bot will be restricted to this channel): ");
                string channel = Console.ReadLine();

                _config = new BotConfiguration
                {
                    Token = token,
                    FilterChannel = channel
                };

                await _cfgMgr.SaveConfig(_config, "dev");
            }
            _config = await _cfgMgr.GetConfig<BotConfiguration>("dev");
#else
            if (!_cfgMgr.ConfigExists<BotConfiguration>())
            {
                Console.Write("Please enter a bot token: ");
                string token = Console.ReadLine();
                _config = new BotConfiguration
                {
                    Token = token
                };

                _cfgMgr.SaveConfig(_config);
            }
            _config = await _cfgMgr.GetConfig<BotConfiguration>();
#endif

            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                AudioMode = Discord.Audio.AudioMode.Disabled,
                LogLevel = LogSeverity.Debug,
                MessageCacheSize = 2000
            });

            _map.Add(_client);

            AddAssemblyActions(_map);

            _client.Log += _client_LogAsync;

            // handle logging to channel
            _client.UserJoined += ChannelLog_UserJoinAsync;
            _client.UserLeft += ChannelLog_UserLeaveAsync;
            _client.UserBanned += ChannelLog_UserBannedAsync;
            _client.UserUnbanned += ChannelLog_UserUnbanned;
            _client.MessageDeleted += ChannelLog_MessageDeletedAsync;
            _client.MessageReceived += _client_MessageRecieved;

            await _client.LoginAsync(TokenType.Bot, _config.Token);
            await _client.ConnectAsync();

            await _client.SetGame(_config.Game);
            if (_config.EnableLoggingChannel)
                _client_SetUpLoggingChannelForNLog();

            await Task.Delay(-1);
        }

        private void _client_SetUpLoggingChannelForNLog()
        {
            try
            {
                if (_config.LoggingGuildID.HasValue && _config.LoggingChannelID.HasValue)
                {
                    _discordNLogTarget = new DiscordNLogTarget(_config.LoggingGuildID.Value, _config.LoggingChannelID.Value, _client);
                    LogManager.Configuration.AddTarget(_discordNLogTarget);
                    LogManager.Configuration.AddRule(LogLevel.Info, LogLevel.Fatal, _discordNLogTarget);
                    _discordNLogTarget.Layout = Layout.FromString(_config.LoggingNLogLayout);
                    LogManager.ReconfigExistingLoggers();
                }
                else
                    BotLogger.Error("Logging Channel and Guild have not been configured! You won't be able to view log on Discord.");
            }
            catch (Exception ex)
            {
                BotLogger.Error(ex, ex.Message);
            }
        }

        private Task _client_LogAsync(LogMessage arg)
        {
            BotLogger.Log(NLogUtility.FromLogSeverity(arg.Severity), arg.ToString());
            return Task.CompletedTask;
        }

        private async Task _client_MessageRecieved(SocketMessage msg)
        {
            IGuildUser user = msg.Author as IGuildUser;
            if (user == null)
                return;
            if (Modules.ModerationExtensions.IsCorrectRole(user, user.Guild, Modules.ModerationModule.StandardRoles) && msg.Content.Equals("~actions enable commandhandler", StringComparison.OrdinalIgnoreCase))
            {
                var success = MessageActions.TryGetValue("CommandHandler", out var commandHandler);
                if (!success)
                    BotLogger.Error("Internal error, couldn't find the command handler action");
                else if (commandHandler.IsEnabled)
                    await msg.Channel.SendMessageAsync("The command handler is already enabled");
                else
                {
                    commandHandler.Enable();
                    await msg.Channel.SendMessageAsync("Enabled commands");
                }
            }
        }

        private Task ChannelLog_MessageDeletedAsync(ulong arg1, Optional<SocketMessage> arg2)
        {
            BotLogger.Info($"Message {arg1} was deleted.");
            if (arg2.IsSpecified)
                BotLogger.Info($"The message was provided:\n{arg2.Value.Content}");
            return Task.CompletedTask;
        }

        private Task ChannelLog_UserUnbanned(SocketUser arg1, SocketGuild arg2)
        {
            BotLogger.Info($"\"{arg1.Username}\" was unbanned from \"{arg2.Name}\"");
            return Task.CompletedTask;
        }

        private Task ChannelLog_UserBannedAsync(SocketUser arg1, SocketGuild arg2)
        {
            BotLogger.Info($"\"{arg1.Username}\" was banned from \"{arg2.Name}\"");
            return Task.CompletedTask;
        }

        private Task ChannelLog_UserLeaveAsync(SocketGuildUser arg)
        {
            BotLogger.Info($"User left: Bye \"{arg.Username}\"!");
            return Task.CompletedTask;
        }

        private Task ChannelLog_UserJoinAsync(SocketGuildUser arg)
        {
            BotLogger.Info($"User joined: Welcome \"{arg.Username}\" to the server!");
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
            MessageActions.TryAdd(handler.GetType().Name, handler);
        }

        private void AddAssemblyActions(IDependencyMap map, bool autoEnable = true)
        {
            var allActions = Assembly.GetEntryAssembly().GetTypes().Where(d => d.GetInterfaces().Contains(typeof(IMessageAction)));

            foreach (var type in allActions)
            {
                AddAction(type, map, autoEnable);
                BotLogger.Debug("Action Added: {0}", type.FullName);
            }
        }
    }
}
