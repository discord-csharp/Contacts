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
        [MaxLength(8000, ErrorMessage ="Username cannot exceed 8000 characters.")]
        public string Username { get; set; }
        public long KarmaCount { get; set; }
    }
}
