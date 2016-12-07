using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ContactsBot.Data
{
    [Table("Karmas", Schema = "ContactsBotSchema")]
    public class Karma
    {
        [Key]
        public string Username { get; set; }
        public long KarmaCount { get; set; }
    }
}
