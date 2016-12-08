using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContactsBot.Modules.Memos
{
    [Table("Memos", Schema = "ContactsBotSchema")]
    public class Memo
    {
        [Key]
        [MaxLength(8000, ErrorMessage = "Username cannot exceed 8000 characters.")]
        public string Key { get; set; }
        [MaxLength(8000, ErrorMessage = "Message cannot exceed 8000 characters.")]
        public string Message { get; set; }
        [MaxLength(8000, ErrorMessage = "Username cannot exceed 8000 characters.")]
        public string CreatedBy { get; set; }
    }
}
