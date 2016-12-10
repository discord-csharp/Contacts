using Discord.Commands;

namespace ContactsBot
{
    public interface IMessageAction
    {
        void Install(IDependencyMap map);

        void Enable();

        void Disable();

        bool IsEnabled { get; }
    }
}
