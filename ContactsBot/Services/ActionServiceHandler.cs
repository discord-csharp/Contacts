using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;

namespace ContactsBot.ActionServices
{
    public class ActionServiceHandler
    {
        public Dictionary<string, ActionService> Services = new Dictionary<string, ActionService>(StringComparer.CurrentCultureIgnoreCase);
        List<ActionService> _enabledServices = new List<ActionService>();
        DiscordSocketClient _client;
        CommandService _command;

        public ActionServiceHandler(IDependencyMap map)
        {
            _client = map.Get<DiscordSocketClient>();
            _command.AddModuleAsync<ActionServiceHandlerCommands>().Wait();
        }

        public bool Enable(string service)
        {
            if(Services.TryGetValue(service, out var result) && !_enabledServices.Contains(result))
            {
                _enabledServices.Add(result);
                return true;
            }
            return false;
        }

        public bool Disable(string service)
        {
            if (Services.TryGetValue(service, out var result) && _enabledServices.Contains(result))
            {
                _enabledServices.Remove(result);
                return true;
            }
            return false;
        }

        public bool Status(string service)
        {
            if (Services.TryGetValue(service, out var result))
                return _enabledServices.Contains(result);
            return false;
        }
    }
}
