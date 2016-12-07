using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContactsBot.Data
{
    [Table("Memos", Schema = "ContactsBotSchema")]
    public class Memo
    {
        [Key]
        public string Key { get; set; }
        public string Message { get; set; }
        public string CreatedBy { get; set; }
    }
}
