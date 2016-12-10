using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ContactsBot.Configuration
{
    public class ConfigManager
    {
        private ConcurrentDictionary<Type, object> _firstLoaded = new ConcurrentDictionary<Type, object>();
        public string RootPath { get; private set; }

        public ConfigManager(string rootPath)
        {
            RootPath = rootPath;
            Directory.CreateDirectory(RootPath);
        }

        public string GetPathToConfig<T>(string name = "default") => Path.Combine(RootPath, typeof(T).Name, name + ".json");

        public bool ConfigExists<T>(string name = "default") => File.Exists(GetPathToConfig<T>(name));

        public async Task<T> GetConfig<T>(string name = "default", bool forceReload = false) where T : class
        {
            if (_firstLoaded.ContainsKey(typeof(T)) && name == "default" && !forceReload)
                return _firstLoaded[typeof(T)] as T;

            if (forceReload)
            {
                object outvar;
                _firstLoaded.TryRemove(typeof(T), out outvar);
            }

            string path = GetPathToConfig<T>(name);
            if (!File.Exists(path))
            {
                await SaveConfig(Activator.CreateInstance(typeof(T)) as T, name: name);
                throw new FileNotFoundException("Skeleton Config file created, please exit program and modify the config file as necessary.");
            }
            else
                using (StreamReader reader = new StreamReader(File.OpenRead(path)))
                {
                    var deserialized = JsonConvert.DeserializeObject<T>(await reader.ReadToEndAsync());

                    if (!_firstLoaded.ContainsKey(typeof(T)))
                    {
                        if (!File.Exists(path))
                        {
                            return null;
                        }

                        _firstLoaded.TryAdd(typeof(T), deserialized);
                    }

                    return deserialized;
                }
        }

        public async Task SaveConfig<T>(T config, string name = "default") where T : class
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
