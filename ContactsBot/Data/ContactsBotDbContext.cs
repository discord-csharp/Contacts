using ContactsBot.Configuration;
using Microsoft.EntityFrameworkCore;
using NLog;
using System;

namespace ContactsBot.Data
{
    public class ContactsBotDbContext : DbContext
    {
        public DbSet<Memo> Memos { get; set; }
        public DbSet<Karma> Karmas { get; set; }
        public DbSet<Log> Logs { get; set; }
        private PostgreSQLConfiguration DbConfiguration { get; set; }
        private static Logger ContactsBotDbContextLogger { get; } = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// For migration purpose only.
        /// </summary>
        public ContactsBotDbContext()
        {
            var config = new ConfigManager("Configs");
#if DEV
            DbConfiguration = config.GetConfig<PostgreSQLConfiguration>("dev").Result;
#else
            DbConfiguration = config.GetConfig<PostgreSQLConfiguration>().Result;
#endif
        }

        public ContactsBotDbContext(PostgreSQLConfiguration config)
        {
            DbConfiguration = config;
        }

        public ContactsBotDbContext(ConfigManager config)
        {
            if (config == null)
                throw new NullReferenceException("ConfigManager was null");

#if DEV
            if (config.ConfigExists<PostgreSQLConfiguration>(name: "dev"))
            {
                DbConfiguration = config.GetConfig<PostgreSQLConfiguration>(name: "dev").GetAwaiter().GetResult();
            }
#else
            if (config.ConfigExists<PostgreSQLConfiguration>())
            {
                DbConfiguration = config.GetConfig<PostgreSQLConfiguration>().GetAwaiter().GetResult();
            }
#endif
            else
            {
                DbConfiguration = new PostgreSQLConfiguration();
#if DEV
                config.SaveConfig(DbConfiguration, name: "dev").Wait();
#else
                config.SaveConfig(DbConfiguration).Wait();
#endif

                var error = new ArgumentException("You must edit the newly created database config file & restart Contacts."
                    + Environment.NewLine
                    + config.GetPathToConfig<PostgreSQLConfiguration>());

                ContactsBotDbContextLogger.Error("You must edit the newly created database config file & restart Contacts.", error);
                throw error;
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(string.Format("User ID={0};Password={1};Host={2};Port={3};Database={4};",
                DbConfiguration.Username,
                DbConfiguration.Password,
                DbConfiguration.Server,
                DbConfiguration.Port,
                DbConfiguration.Database));
        }
    }
}
