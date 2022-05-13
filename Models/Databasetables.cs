using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpenceTracker.Models
{
    public class User
    {
        [Key]
        public int uid { get; set; }
        public string fullname { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string role { get; set; }
    }

    public class Accountscatagory
    {
        [Key]
        public int catid { get; set; }
        public string catagory { get; set; }
        public bool iscapital { get; set; }
        public long code { get; set; }
    }

    public class Accountshead
    {
        [Key]
        public int accid { get; set; } 
        [StringLength(50)]       
        public string expence { get; set; }
        public long code { get; set; }
        [ForeignKey("catid")]
        public int catid { get; set; }
        public virtual Accountscatagory cat { get; set; }
        public virtual ICollection<Transaction> trans { get; set; }
    }
    public class Transaction
    {
        [Key]
        public int trnid { get; set; }
        public DateTime trxdate { get; set; }
        public long trxno { get; set; }
        public string remarks { get; set; }
        public decimal debit { get; set; }
        public decimal credit { get; set; }
        public decimal balance { get; set; }
      
        [ForeignKey("accid")]
        public int accid { get; set; }
        public virtual Accountshead exp { get; set; }
    }
}
