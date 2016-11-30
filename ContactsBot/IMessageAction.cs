using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace ContactsBot
{
    interface IMessageAction
    {
        void Install(IDependencyMap map);

        void Enable();

        void Disable();

        bool IsEnabled { get; }
    }
}
