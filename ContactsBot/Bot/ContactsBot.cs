using ContactsBot.Configuration;
using ContactsBot.NLogTargets;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using NLog;
using NLog.Layouts;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ContactsBot
{
    public class ContactsBot
    {
        DiscordSocketClient _client;
        DependencyMap _map;
        GlobalConfiguration _config;
        ConfigManager _cfgMgr;
        internal static Logger BotLogger { get; set; }
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
            
            _config = await _cfgMgr.GetConfigAsync<GlobalConfiguration>("global");

            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                AudioMode = Discord.Audio.AudioMode.Disabled,
                LogLevel = LogSeverity.Debug,
                MessageCacheSize = 2000
            });

            _map.Add(_client);
            


            _client.Log += Client_LogAsync;

            // handle logging to channel
            _client.UserJoined += ChannelLog_UserJoinAsync;
            _client.UserLeft += ChannelLog_UserLeaveAsync;
            _client.UserBanned += ChannelLog_UserBannedAsync;
            _client.UserUnbanned += ChannelLog_UserUnbanned;
            _client.MessageDeleted += ChannelLog_MessageDeletedAsync;
            _client.MessageReceived += Client_MessageRecievedAsync;

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
    }
}
