using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContactsBot.Data
{
    [Table("Karmas", Schema = "ContactsBotSchema")]
    public class Karma
    {
        [Key]
        public long UserID { get; set; }
        public int KarmaCount { get; set; }
    }
}
