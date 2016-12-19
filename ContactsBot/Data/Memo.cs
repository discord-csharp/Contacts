using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContactsBot.Data
{
    [Table("Memos", Schema = "ContactsBotSchema")]
    public class Memo
    {
        [Key, MaxLength(50, ErrorMessage = "Memo name cannot exceed 50 characters."), MinLength(1, ErrorMessage = "Memo name cannot be empty.")]
        public string MemoName { get; set; }
        [MaxLength(500, ErrorMessage = "Message cannot exceed 500 characters."), MinLength(1, ErrorMessage = "Memo must contains a message!")]
        public string Message { get; set; }
        public long UserID { get; set; }
    }
}
