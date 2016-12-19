using NLog;
using NLog.Config;
using System;

namespace ContactsBot
{
    static class Program
    {
        static Logger ProgramLogger { get; } = LogManager.GetCurrentClassLogger();
        static void Main(string[] args)
        {
            LogManager.ThrowConfigExceptions = true;
#if DEV
            LogManager.Configuration = new XmlLoggingConfiguration("NLog.Dev.config");
#else
            LogManager.Configuration = new XmlLoggingConfiguration("NLog.config");
#endif
            LogManager.EnableLogging();
            try
            {
                new ContactsBot().RunBotAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                ProgramLogger.Error(ex);
            }
            Console.WriteLine("Bot Teriminated.");
            Console.ReadLine();
        }
    }
}