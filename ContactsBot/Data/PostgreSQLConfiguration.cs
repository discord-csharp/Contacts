using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ContactsBot.Data
{
    internal static class PostgreSQLGlobalConfig
    {
        public static bool InitializePostgreSQLGlobalConfig()
        {
            try
            {
                if (File.Exists("Postgres.json"))
                    Config = JsonConvert.DeserializeObject<PostgreSQLConfiguration>(File.ReadAllText("Postgres.json"));
                else
                {
                    Config = new PostgreSQLConfiguration();
                    File.WriteAllText("Postgres.json", JsonConvert.SerializeObject(Config));
                    Console.WriteLine("Postgres.json has been created, please add PostgreSQL Database Server information.");
                    Console.WriteLine("Also please execute PostgreSQL Script for specific database you created for this bot.");
                    Console.WriteLine("The script is for creating necessary Schema and Tables.");
                    Console.ReadLine();
                    return false;
                }
            }
            catch (Exception ex)
            {
                Config = new PostgreSQLConfiguration();
                Console.WriteLine(ex.Message);
                Console.ReadLine();
                return false;
            }
            return true;
        }
        internal static PostgreSQLConfiguration Config { get; set; }
    }

    public class PostgreSQLConfiguration
    {
        public string PostgreSQLServer { get; set; } = "localhost";
        public ushort PostgreSQLServerPort { get; set; } = 5433;
        public string PostgreSQLUsername { get; set; } = "postgres";
        public string PostgreSQLPassword { get; set; } = "postgres";
        public string PostgreSQLDatabase { get; set; } = "default";
    }
}
