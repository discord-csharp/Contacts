using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;

namespace ContactsBot.Configuration
{
    public class ConfigManager
    {
        public string RootPath { get; private set; }

        public ConfigManager(string rootPath)
        {
            RootPath = rootPath;
            Directory.CreateDirectory(RootPath);
        }

        public string GetPathToConfig<T>(string name) => Path.Combine(RootPath, typeof(T).Name, name + ".json");

        public bool ConfigExists<T>(string name) => File.Exists(GetPathToConfig<T>(name));

        public async Task<T> GetConfigAsync<T>(string name) where T : class
        {
            string path = GetPathToConfig<T>(name);
            if (!File.Exists(path))
            {
                await SaveConfigAsync(Activator.CreateInstance(typeof(T)) as T, name: name);
                throw new FileNotFoundException("Skeleton Config file created, please exit program and modify the config file as necessary.");
            }
            else
                using (StreamReader reader = new StreamReader(File.OpenRead(path)))
                    return JsonConvert.DeserializeObject<T>(await reader.ReadToEndAsync());
        }

        public async Task SaveConfigAsync<T>(T config, string name) where T : class
        {
            string serialized = JsonConvert.SerializeObject(config, Formatting.Indented);
            string path = GetPathToConfig<T>(name);

            Directory.CreateDirectory(Path.GetDirectoryName(path));

            using (StreamWriter writer = new StreamWriter(new FileStream(GetPathToConfig<T>(name), FileMode.Create)))
            {
                await writer.WriteAsync(serialized);
            }
        }
    }
}
