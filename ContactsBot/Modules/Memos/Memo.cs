﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContactsBot.Modules.Memos
{
    [Table("Memos", Schema = "DefaultSchema")]
    public class Memo
    {
        [Key]
        public string Key { get; set; }
        public string Message { get; set; }
        public string CreatedBy { get; set; }
    }
}