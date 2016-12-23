using ContactsBot.Configuration;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using NLog;
using System.Reflection;
using System.Linq;
using System.Collections.Concurrent;
using ContactsBot.NLogTargets;
using NLog.Layouts;
using ContactsBot.Data;
using Newtonsoft.Json;

namespace ContactsBot
{
    public class ContactsBot : IBotInterface
    {
        DiscordSocketClient _client;
        DependencyMap _map;
        BotConfiguration _config;
        ConfigManager _cfgMgr;
        internal static Logger BotLogger { get; set; }
        public ConcurrentDictionary<IGuildUser, DateTime> MutedUsers { get; } = new ConcurrentDictionary<IGuildUser, DateTime>();
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
                await _cfgMgr.SaveConfigAsync(new BotConfiguration(), "dev");
                BotLogger.Info("No dev.json configuration exists for BotConfiguration. Skeleton File generated. Please read the README.md for installation guide.");
                return;
            }
            _config = await _cfgMgr.GetConfigAsync<BotConfiguration>("dev");
#else
            if (!_cfgMgr.ConfigExists<BotConfiguration>())
            {
                await _cfgMgr.SaveConfigAsync(new BotConfiguration());
                BotLogger.Info("No default.json configuration exists for BotConfiguration. Skeleton File generated. Please read the README.md for installation guide.");
                return;
            }
            _config = await _cfgMgr.GetConfigAsync<BotConfiguration>();
#endif

            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                AudioMode = Discord.Audio.AudioMode.Disabled,
                LogLevel = LogSeverity.Debug,
                MessageCacheSize = 2000
            });

            _map.Add(_client);

            AddAssemblyActions(_map);

            _client.Log += Client_LogAsync;

            // handle logging to channel
            _client.UserJoined += ChannelLog_UserJoinAsync;
            _client.UserLeft += ChannelLog_UserLeaveAsync;
            _client.UserBanned += ChannelLog_UserBannedAsync;
            _client.UserUnbanned += ChannelLog_UserUnbanned;
            _client.MessageDeleted += ChannelLog_MessageDeletedAsync;
            _client.MessageReceived += Client_MessageRecievedAsync;
            _client.ReactionAdded += OnReactionAdded;
            _client.ReactionRemoved += OnReactionRemoved;
            

            await _client.LoginAsync(TokenType.Bot, _config.Token);
            await _client.ConnectAsync();

            await _client.SetGame(_config.Game);
            if (_config.EnableLoggingChannel)
                Client_SetUpLoggingChannelForNLog();

            await Task.Delay(-1);
        }

        private void Client_SetUpLoggingChannelForNLog()
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

        private Task Client_LogAsync(LogMessage arg)
        {
            switch (arg.Severity)
            {
                case LogSeverity.Critical:
                    {
                        BotLogger.Log(LogLevel.Fatal, arg.ToString());
                        break;
                    }
                case LogSeverity.Debug:
                    {
                        BotLogger.Log(LogLevel.Debug, arg.ToString());
                        break;
                    }
                case LogSeverity.Error:
                    {
                        BotLogger.Log(LogLevel.Error, arg.ToString());
                        break;
                    }
                case LogSeverity.Info:
                    {
                        BotLogger.Log(LogLevel.Info, arg.ToString());
                        break;
                    }
                case LogSeverity.Verbose:
                    {
                        BotLogger.Log(LogLevel.Trace, arg.ToString());
                        break;
                    }
                case LogSeverity.Warning:
                    {
                        BotLogger.Log(LogLevel.Warn, arg.ToString());
                        break;
                    }
            }
            return Task.CompletedTask;
        }

        private async Task Client_MessageRecievedAsync(SocketMessage msg)
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
        
        [Summary("This event will keep track of Karma given to users")]
        private async Task OnReactionAdded(ulong arg1, Optional<SocketUserMessage> arg2, SocketReaction arg3)
        {
            if (arg3.Emoji.Name == "👍")
            {
                try
                {
                    using (var context = new ContactsBotDbContext())
                    {
                        var databaseUserID = (long)user.Id;
                        var item = context.Karmas.FirstOrDefault(I => I.UserID == databaseUserID);
                        if (item != null)
                        {
                            item.KarmaCount++;
                            context.Karmas.Update(item);
                        }
                        else
                            await context.Karmas.AddAsync(new Karma() { UserID = (long)user.Id, KarmaCount = 1 });
                        await context.SaveChangesAsync();
                        await arg3.Channel.SendMessageAsync($"Thanks {user.Username}");
                    }
                }
                catch (Exception ex)
                {
                    KarmasModuleLogger.Error(ex, ex.Message);
                }
            }
        }
        
        [Summary("This event will keep track of Karma removed from users")]
        private async Task OnReactionRemoved(ulong arg1, Optional<SocketUserMessage> arg2, SocketReaction arg3)
        {

            if (arg3.Emoji.Name == "👍")
            {
                try
                {
                    using (var context = new ContactsBotDbContext())
                    {
                        var databaseUserID = (long)user.Id;
                        var item = context.Karmas.FirstOrDefault(I => I.UserID == databaseUserID);
                        item.KarmaCount--;
                        context.Karmas.Update(item);                       
                        await context.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    KarmasModuleLogger.Error(ex, ex.Message);
                }
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
