using Discord.Commands;

namespace ContactsBot.ActionServices
{
    [Group("actions"), DontAutoLoad]
    public class ActionServiceHandlerCommands : ModuleBase
    {
        [Command]
        public void List()
        {

        }
    }
}
