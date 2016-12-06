using Microsoft.EntityFrameworkCore;

namespace ContactsBot.Data
{
    public class ContactsBotDbContext : DbContext
    {
        public DbSet<Memo> Memos { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(string.Format("User ID={0};Password={1};Host={2};Port={3};Database={4};",
                PostgreSQLGlobalConfig.Config.PostgreSQLUsername,
                PostgreSQLGlobalConfig.Config.PostgreSQLPassword,
                PostgreSQLGlobalConfig.Config.PostgreSQLServer,
                PostgreSQLGlobalConfig.Config.PostgreSQLServerPort,
                PostgreSQLGlobalConfig.Config.PostgreSQLDatabase));
        }
    }
}
