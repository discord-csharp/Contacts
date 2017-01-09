using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

namespace ContactsBot.Services
{
    public class MuteService
    {
        ConcurrentDictionary<IGuildUser, DateTime> _mutedUsers = new ConcurrentDictionary<IGuildUser, DateTime>();
        Timer _pollTimer;

        public MuteService(DiscordSocketClient client)
        {
            _pollTimer = new Timer(PollMutedUsers, null, new TimeSpan(0), new TimeSpan(0, 0, 10));
        }

        public bool Mute(IGuildUser user)
        {
            if (user == null) return false;
        }

        public bool Unmute(IGuildUser user)
        {

        }

        private void PollMutedUsers(object state)
        {
            if (_mutedUsers.IsEmpty) return;

            foreach(var user in _mutedUsers.Where(u => u.Value <= DateTime.UtcNow))
                Unmute(user.Key);
        }
    }
}
