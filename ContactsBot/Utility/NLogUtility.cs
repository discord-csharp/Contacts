using Discord;
using NLog;

namespace ContactsBot.Utility
{
    internal static class NLogUtility
    {
        /// <summary>
        /// Return an equivalent NLog Log Level for Discord Log Severity Level
        /// </summary>
        /// <param name="level">Discord Log Severity Level</param>
        /// <returns>NLog Log Level</returns>
        public static LogLevel FromLogSeverity(LogSeverity level)
        {
            switch (level)
            {
                case LogSeverity.Critical:
                    {
                        return LogLevel.Fatal;
                    }
                case LogSeverity.Debug:
                    {
                        return LogLevel.Debug;
                    }
                case LogSeverity.Error:
                    {
                        return LogLevel.Error;
                    }
                case LogSeverity.Info:
                    {
                        return LogLevel.Info;
                    }
                case LogSeverity.Verbose:
                    {
                        return LogLevel.Trace;
                    }
                case LogSeverity.Warning:
                    {
                        return LogLevel.Warn;
                    }
                default:
                    {
                        return LogLevel.Off;
                    }
            }
        }
    }
}
