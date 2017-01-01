using ContactsBot.Configuration;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ContactsBot.Modules
{
    [Group("configmod")]
    [Name("Config")]
    [Summary("Allows for editing of the server's bot config file from chat.")]
    [RequireUserPermission(GuildPermission.BanMembers)]
    public class ConfigModule : ModuleBase
    {
        PropertyInfo[] _properties = typeof(ServerBotConfiguration).GetProperties();
        ConcurrentDictionary<ulong, ServerBotConfiguration> _botConfigs;
        ConfigManager _configManager;

        public ConfigModule(IDependencyMap _map)
        {
            _botConfigs = _map.Get<ConcurrentDictionary<ulong, ServerBotConfiguration>>();
            _configManager = _map.Get<ConfigManager>();
        }

        [Command]
        public async Task ListAsync()
        {
            string message = "Editable bot properties: ";
            for(int i = 0; i < _properties.Length - 1; i++)
            {
                message += $"{_properties[i].Name}, ";
            }
            message += _properties.LastOrDefault()?.Name;
            await ReplyAsync(message);
        }

        [Command]
        public async Task GetValueAsync(string propertyName)
        {
            var property = _properties.FirstOrDefault(p => string.Equals(p.Name, propertyName, System.StringComparison.CurrentCultureIgnoreCase));
            if(property is null)
            {
                await ReplyAsync($"Couldn't find {propertyName}");
                return;
            }
            _botConfigs.TryGetValue(Context.Guild.Id, out var config);
            await ReplyAsync($"{property.Name} ({property.PropertyType}) = ``{property.GetValue(config)?.ToString() ?? "null"}``");
        }

        [Command]
        public async Task SetValue(string propertyName, string value)
        {
            var property = _properties.FirstOrDefault(p => string.Equals(p.Name, propertyName, System.StringComparison.CurrentCultureIgnoreCase));
            if (property is null)
            {
                await ReplyAsync($"Couldn't find {propertyName}");
                return;
            }
            _botConfigs.TryGetValue(Context.Guild.Id, out var config);
            var type = property.PropertyType;
            object result = Convert.ChangeType(value, type);

            property.SetValue(config, result);
            await ReplyAsync($"{property.Name} ({type}) = ``{value}``");
        }

        [Command(nameof(ServerBotConfiguration.FilterChannels))]
        public async Task SetFilterChannels(params ulong[] channels)
        {
            _botConfigs.TryGetValue(Context.Guild.Id, out var config);
            config.FilterChannels = channels;
        }

        [Command("save")]
        public async Task Save()
        {
            _botConfigs.TryGetValue(Context.Guild.Id, out var config);
            await _configManager.SaveConfigAsync(config, Context.Guild.Id.ToString());
            await ReplyAsync($"Config saved for {Context.Guild.Name}");
        }
    }
}
