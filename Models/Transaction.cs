using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Bank.Models
{
    public class Transaction
    {
        [Key]
        public int TransId { get; set; }
        public string Type { get; set; }
        public double Amount { get; set; }
        public int AccountId { get; set; }
        public DateTime transTime { get; set; }

        public virtual Account MyAccount { get; set; }
    }
}
