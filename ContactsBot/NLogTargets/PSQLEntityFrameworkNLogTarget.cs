using ContactsBot.Configuration;
using ContactsBot.Data;
using NLog;
using NLog.Targets;
using System;

namespace ContactsBot.NLogTargets
{
    /// <summary>
    /// An Internally Developed NLog Target that uses Entity Frameworks for
    /// Logging datas
    /// </summary>
    [Target("PSQLEntityFramework")]
    public sealed class PSQLEntityFrameworkNLogTarget : TargetWithLayout
    {
        private PostgreSQLConfiguration DbConfig { get; set; }
        private static Logger PSQLEntityLogger { get; } = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// Attempts to retrieve PostgreSQL configuration information
        /// independently from ContactsBot instance.
        /// </summary>
        public PSQLEntityFrameworkNLogTarget()
        {
            var config = new ConfigManager("Configs");
            try
            {
#if DEV
                DbConfig = config.GetConfigAsync<PostgreSQLConfiguration>(name: "dev").GetAwaiter().GetResult();
#else
                DbConfig = config.GetConfigAsync<PostgreSQLConfiguration>().GetAwaiter().GetResult();
#endif
            }
            catch (Exception ex)
            {
                PSQLEntityLogger.Error(ex, ex.Message);
            }
        }

        protected override void Write(LogEventInfo info)
        {
            using (var context = new ContactsBotDbContext(DbConfig))
            {
                context.Logs.Add(new Log
                {
                    Level = info.Level.Name,
                    Exception = info.Exception?.ToString(),
                    Message = Layout.Render(info),
                    Timestamp = info.TimeStamp.ToUniversalTime()
                });
                context.SaveChanges();
            }
        }
    }
}
