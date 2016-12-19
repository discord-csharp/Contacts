using Discord.Commands;

namespace ContactsBot
{
    public interface IMessageAction
    {
        void InstallAsync(IDependencyMap map);

        void Enable();

        void Disable();

        bool IsEnabled { get; }
    }
}
