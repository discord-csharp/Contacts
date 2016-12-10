using System;
using System.Collections.Generic;
using System.Text;

namespace ContactsBot.Configuration
{
    public class PostgreSQLConfiguration
    {
        public string Server { get; set; } = "localhost";
        public ushort Port { get; set; } = 5433;
        public string Username { get; set; } = "postgres";
        public string Password { get; set; } = "postgres";
        public string Database { get; set; } = "testdatabase";
    }
}
