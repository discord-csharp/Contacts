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
        static Global()
        {
            Memos = new ConcurrentDictionary<string, string>();
            if (File.Exists("memos.json"))
            {
                var enumerate = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("memos.json")).GetEnumerator();
                while (enumerate.MoveNext())
                {
                    Memos.TryAdd(enumerate.Current.Key, enumerate.Current.Value);
                }
            }

            GlobalMemosWriteTimer = new Timer(delegate (object state)
            {
                if (!NewDataWritten) return;
                NewDataWritten = false;
                var dictionary = new Dictionary<string, string>();
                foreach (var pair in Memos.ToArray())
                    dictionary.Add(pair.Key, pair.Value);

                File.WriteAllText("memos.json", JsonConvert.SerializeObject(dictionary, Formatting.Indented));

            }, null, new TimeSpan(0, 10, 0), new TimeSpan(0, 10, 0) );
        }

        internal static ConcurrentDictionary<IGuildUser, Timer> MutedUsers = new ConcurrentDictionary<IGuildUser, Timer>();

        // actions
        internal static ConcurrentDictionary<string, IMessageAction> MessageActions { get; } = new ConcurrentDictionary<string, IMessageAction>(StringComparer.OrdinalIgnoreCase);

        // memos
        internal static ConcurrentDictionary<string, string> Memos { get; }
        internal static volatile bool NewDataWritten = false;
        internal static Timer GlobalMemosWriteTimer { get; }
    }
}
