using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContactsBot.Data
{
    [Table("Logs", Schema = "ContactsBotSchema")]
    public class Log
    {
        [Key]
        public long LogID { get; set; }
        [Required]
        [MaxLength(5, ErrorMessage = "Level string cannot exceed 5 characters.")]
        public string Level { get; set; }
        [Required]
        public DateTime Timestamp { get; set; }
        public string Message { get; set; }
        public string Exception { get; set; }
    }
}
