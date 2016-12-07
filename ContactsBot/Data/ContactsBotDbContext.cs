using ContactsBot.Configuration;
using ContactsBot.Modules.Memos;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace ContactsBot.Data
{
    public class ContactsBotDbContext : DbContext
    {
        public DbSet<Memo> Memos { get; set; }
        public DbSet<Karma> Karmas { get; set; }

        private ConfigManager _config;

        public ContactsBotDbContext(ConfigManager config)
        {
            _config = config;

            if (_config == null)
            {
                throw new NullReferenceException("ConfigManager was null");
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            PostgreSQLConfiguration config;

            if (_config.ConfigExists<PostgreSQLConfiguration>())
            {
                config = _config.GetConfig<PostgreSQLConfiguration>().Result;
            }
            else
            {
                config = new PostgreSQLConfiguration();
                _config.SaveConfig(config);

                throw new ArgumentException("You must edit the newly created database config file & restart Contacts."
                    + Environment.NewLine
                    + _config.GetPathToConfig<PostgreSQLConfiguration>());
            }

            optionsBuilder.UseNpgsql(string.Format("User ID={0};Password={1};Host={2};Port={3};Database={4};",
                config.Username, config.Password, config.Server, config.Port, config.Database));
        }

        public class PostgreSQLConfiguration
        {
            public string Server { get; set; } = "localhost";
            public ushort Port { get; set; } = 5433;
            public string Username { get; set; } = "postgres";
            public string Password { get; set; } = "postgres";
            public string Database { get; set; } = "default";
        }
    }
}
