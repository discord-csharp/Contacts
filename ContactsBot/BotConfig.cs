using Discord.WebSocket;
using Newtonsoft.Json;
using System.IO;

namespace Contacts
{
    public class BotConfiguration : DiscordSocketConfig
    {
        public string Token { get; set; }

        public static BotConfiguration ProcessBotConfig(string path) => JsonConvert.DeserializeObject<BotConfiguration>(File.ReadAllText(path));

        public void SaveBotConfig(string path) => File.WriteAllText(path, JsonConvert.SerializeObject(this));

        public static BotConfiguration CreateBotConfigWithToken(string path, string token, DiscordSocketConfig socketConfig = null)
        {
            socketConfig = socketConfig ?? new DiscordSocketConfig();
            BotConfiguration config = socketConfig as BotConfiguration;
            config.Token = token;
            config.SaveBotConfig(path);
            return config;
        }
    }
}
