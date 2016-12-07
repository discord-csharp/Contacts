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
        internal static Dictionary<string, IMessageAction> MessageActions { get; } = new Dictionary<string, IMessageAction>(StringComparer.OrdinalIgnoreCase);

        // memos
        internal static Dictionary<string, string> Memos { get; } = (File.Exists("memos.json")) ? JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("memos.json")) : new Dictionary<string, string>();

        internal static int IgnoreCount { get; set; }
    }
}
