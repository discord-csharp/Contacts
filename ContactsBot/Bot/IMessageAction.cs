using Discord.Commands;
using System.Threading.Tasks;

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
