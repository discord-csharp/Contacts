using NLog;
using NLog.Config;

namespace ContactsBot
{
    static class Program
    {
        static void Main(string[] args)
        {
            LogManager.ThrowConfigExceptions = true;
#if DEV
            LogManager.Configuration = new XmlLoggingConfiguration("NLog.Dev.config");
#else
            LogManager.Configuration = new XmlLoggingConfiguration("NLog.config");
#endif
            LogManager.EnableLogging();
            new ContactsBot().RunBotAsync().GetAwaiter().GetResult();
        }
    }
}