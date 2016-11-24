using Contacts;
using Discord.API;
using Discord.API.Rest;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

class Program
{
    static void Main(string[] args) => new Program().RunBot().GetAwaiter().GetResult();

    DiscordSocketClient _client;
    CommandService _commands;

    public async Task RunBot()
    {
        BotConfiguration config;
        if (!File.Exists("config.json"))
        {
            Console.Write("Please enter a bot token:");
            string token = Console.ReadLine();
            config = BotConfiguration.CreateBotConfigWithToken("config.json", token, new DiscordSocketConfig
            {
                AudioMode = Discord.Audio.AudioMode.Disabled,
                LogLevel = Discord.LogSeverity.Info
            });
        }
        else
            config = BotConfiguration.ProcessBotConfig("config.json");

        _client = new DiscordSocketClient(config as DiscordSocketConfig);

        _client.Ready += _client_Ready;
        _client.MessageReceived += _client_MessageReceived;

        await _client.LoginAsync(Discord.TokenType.Bot, config.Token);

        await _client.ConnectAsync();

        await Task.Delay(-1);
    }

    private async Task ApplyCommands()
    {
        await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
    } 

    private async Task _client_MessageReceived(SocketMessage msg)
    {
        var message = msg as SocketUserMessage;
        if (message == null) return;

        int argPos = 0;

        if(message.HasCharPrefix('~', ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos))
        {
            var context = new CommandContext(_client, message);

            var result = await _commands.ExecuteAsync(context, argPos);
            if (!result.IsSuccess)
                await message.Channel.SendMessageAsync(result.ErrorReason);
        }
        else
        {
            // todo, work on filtering messages
        }
    }

    private async Task _client_Ready()
    {
        await _client.CurrentUser.ModifyAsync(mod => new ModifyCurrentUserParams
        {
            Avatar = new Image(new FileStream("Assets/contacts.png", FileMode.Open))
        });
    }
}