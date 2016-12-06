using Discord;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Collections;

namespace ContactsBot
{
    internal static class Global
    {
        internal static ConcurrentDictionary<IGuildUser, Timer> MutedUsers = new ConcurrentDictionary<IGuildUser, Timer>();

        // actions
        internal static ConcurrentDictionary<string, IMessageAction> MessageActions { get; } = new ConcurrentDictionary<string, IMessageAction>(StringComparer.OrdinalIgnoreCase);
    }
}
