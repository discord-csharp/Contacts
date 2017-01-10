using Discord;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ContactsBot.Services
{
    public class MuteService
    {
        ConcurrentDictionary<KeyValuePair<IGuildUser, IRole>, DateTime> _mutedUsers = new ConcurrentDictionary<KeyValuePair<IGuildUser, IRole>, DateTime>();
        ConcurrentDictionary<IGuild, IRole> _cachedRoles = new ConcurrentDictionary<IGuild, IRole>();
        readonly Timer _pollTimer;

        public MuteService()
        {
            _pollTimer = new Timer(PollMutedUsers, null, new TimeSpan(0), new TimeSpan(0, 0, 10));
        }

        public bool Mute(IGuildUser user, TimeSpan time, IRole muteRole)
        {
            if (user == null || muteRole == null) return false;

            muteRole = _cachedRoles.GetOrAdd(user.Guild, muteRole);
            user.AddRolesAsync(new[] { muteRole });
            return _mutedUsers.TryAdd(new KeyValuePair<IGuildUser, IRole>(user, muteRole), DateTime.UtcNow + time);
        }

        public bool Unmute(IGuildUser user, IRole muteRole)
        {
            if (user == null) return false;
            user.RemoveRolesAsync(new[] { muteRole });
            return _mutedUsers.TryRemove(new KeyValuePair<IGuildUser, IRole>(user, muteRole), out var value);
        }

        private void PollMutedUsers(object state)
        {
            if (_mutedUsers.IsEmpty) return;

            foreach (var user in _mutedUsers.Where(u => u.Value <= DateTime.UtcNow))
                Unmute(user.Key.Key, user.Key.Value);
        }
    }
}
