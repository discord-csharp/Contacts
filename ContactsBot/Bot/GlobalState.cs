using Discord;
using System;
using System.Collections.Concurrent;

namespace ContactsBot
{
    public class GlobalState
    {
        internal ConcurrentDictionary<IGuildUser, DateTime> MutedUsers { get; set; }
        internal ConcurrentDictionary<string, IMessageAction> MessageActions { get; set; }
        internal long IgnoreCount { get; set; }
    }
}