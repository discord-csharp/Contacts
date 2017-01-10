using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ContactsBot.ActionServices
{
    [Group("actions"), DontAutoLoad]
    public class ActionServiceHandlerCommands : ModuleBase
    {
        private ActionServiceHandler _handler;

        public ActionServiceHandlerCommands(ActionServiceHandler handler)
        {
            _handler = handler;
        }

        [Command]
        public async Task ListAsync()
        {
            string reply = string.Empty;
            foreach (var service in _handler.Services)
                reply += $"{service.Key}\n";
        }

        [Command("status")]
        public async Task StatusAsync(string service)
        {
            await ReplyAsync($"{service} enabled? {_handler.Status(service)}");
        }

        [Command("enable")]
        public async Task EnableAsync(string service)
        {
            bool success = _handler.Enable(service);
            await ReplyAsync(success ? $"Successfully enabled {service}" : $"Couldn't enable {service}. It might not exist or it's already enabled");
        }

        [Command("disable")]
        public async Task DisableAsync(string service)
        {
            bool success = _handler.Disable(service);
            await ReplyAsync(success ? $"Successfully disabled {service}" : $"Couldn't disable {service}. It might not exist or it's already disabled");
        }
    }
}
