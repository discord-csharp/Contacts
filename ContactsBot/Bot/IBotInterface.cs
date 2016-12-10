using Discord;
using System.Collections.Concurrent;
using System.Threading;

namespace ContactsBot
{
    /// <summary>
    /// Use this interface for exposing global states to modules
    /// </summary>
    public interface IBotInterface
    {
        ConcurrentDictionary<IGuildUser, Timer> MutedUsers { get; }
        ConcurrentDictionary<string, IMessageAction> MessageActions { get; }
        long IgnoreCount { get; set; }
    }
}
