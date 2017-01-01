using System;
using System.Collections.Generic;
using System.Text;

namespace ContactsBot.Modules
{
    public interface IBotService
    {
        bool Enabled { get; }

        bool Enable();

        bool Disable();
    }
}
