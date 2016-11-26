using Discord;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ContactsBot
{
    internal static class Global
    {
        // muting users
        internal static List<Timer> MutedTimers = new List<Timer>();

        internal static Dictionary<IGuildUser, Timer> MutedUsers = new Dictionary<IGuildUser, Timer>();
    }
}
