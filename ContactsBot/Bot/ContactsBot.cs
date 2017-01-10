using ContactsBot.Configuration;
using ContactsBot.NLogTargets;
using ContactsBot.Services;
using ContactsBot.ActionServices;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using NLog;
using NLog.Layouts;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace ContactsBot
{
    public class ContactsBot
    {
        DiscordSocketClient _client;
        DependencyMap _map;
        GlobalConfiguration _config;
        ConfigManager _cfgMgr;
        MuteService _mute;
        ActionServiceHandler _serviceHandler;
        ConcurrentDictionary<IGuild, ServerConfiguration> _serverConfigurations;
        internal static Logger BotLogger { get; set; }
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
            _mute = new MuteService();

            _map.Add(_cfgMgr);
            _map.Add(this);
            _map.Add(_mute);
            
            _config = await _cfgMgr.GetConfigAsync<GlobalConfiguration>("global");

            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                AudioMode = Discord.Audio.AudioMode.Disabled,
                LogLevel = LogSeverity.Debug,
                MessageCacheSize = 2000
            });

            _map.Add(_client);

            _serviceHandler = new ActionServiceHandler(_map);
            AddServices(Assembly.GetEntryAssembly());
            _map.Add(_serviceHandler);
            
            _client.Log += Client_LogAsync;

            // handle logging to channel
            _client.UserJoined += ChannelLog_UserJoinAsync;
            _client.UserLeft += ChannelLog_UserLeaveAsync;
            _client.UserBanned += ChannelLog_UserBannedAsync;
            _client.UserUnbanned += ChannelLog_UserUnbanned;

            await _client.LoginAsync(TokenType.Bot, _config.Token);
            await _client.ConnectAsync();

            await _client.SetGameAsync(_config.Game);

            await Task.Delay(-1);
        }

        /*
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
*/
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

        private void AddServices(Assembly assembly)
        {
            var types = assembly.GetTypes().Where(type => type == typeof(ActionService));
            
            foreach(var type in types)
            {
                var action = Activator.CreateInstance(type, new[] { _map }) as ActionService;
                // todo: load all previous enabled actions
                _serviceHandler.Services.Add(type.Name, action);
            }
        }
    }
}
