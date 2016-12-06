using Discord;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace ContactsBot
{
    internal static class Global
    {
        // muting users
        internal static List<Timer> MutedTimers = new List<Timer>();

        internal static Dictionary<IGuildUser, Timer> MutedUsers = new Dictionary<IGuildUser, Timer>();

        // actions
        internal static Dictionary<string, IMessageAction> MessageActions { get; } = new Dictionary<string, IMessageAction>(StringComparer.OrdinalIgnoreCase);

        // memos
        internal static Dictionary<string, string> Memos { get; } = (File.Exists("memos.json")) ? JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("memos.json")) : new Dictionary<string, string>();

        // ignore messages
        internal static int IgnoreCount { get; set; 
    }
}
