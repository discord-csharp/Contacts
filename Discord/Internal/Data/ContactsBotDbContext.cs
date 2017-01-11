using ContactsBot.Configuration;
using Microsoft.EntityFrameworkCore;
using NLog;
using System;

namespace ContactsBot.Data
{
    public class ContactsBotDbContext : DbContext
    {
        public DbSet<Log> Logs { get; set; }
        
        private static PostgreSQLConfiguration DbConfiguration { get; } = new ConfigManager("Configs").GetConfigAsync<PostgreSQLConfiguration>().GetAwaiter().GetResult();

        private static Logger ContactsBotDbContextLogger { get; } = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// For migration purpose only.
        /// </summary>
        public ContactsBotDbContext()
        {
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
