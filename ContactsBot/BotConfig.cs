using Discord;
using Discord.Audio;
using Discord.WebSocket;
using Newtonsoft.Json;
using System.IO;

namespace ContactsBot
{
    public class BotConfiguration
    {
        public string Token { get; set; }

        public string DevToken { get; set; }

        public char PrefixCharacter { get; set; } = '~';

        public static BotConfiguration ProcessBotConfig(string path) => JsonConvert.DeserializeObject<BotConfiguration>(File.ReadAllText(path));

        public void SaveBotConfig(string path) => File.WriteAllText(path, JsonConvert.SerializeObject(this));

        public static BotConfiguration CreateBotConfigWithToken(string path, string token = "", string devToken = "")
        {
            BotConfiguration config = new BotConfiguration()
            {
                Token = token,
                DevToken = devToken
            };
            config.SaveBotConfig(path);
            return config;
        }
    }
}
