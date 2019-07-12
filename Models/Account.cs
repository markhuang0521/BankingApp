using Bank.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Bank.Models
{
    public class Account
    {
        [Key]
        [Display (Name="Account Id")]
        public int AccountId { get; set; }
        public string UserId { get; set; }
        [Display(Name = "Account type")]

        public string Type { get; set; }
        public double? InterestRate { get; set; }
        public double? OverDraft { get; set; }
        [Display(Name = "Current Balance")]

        public double Balance { get; set; }
        [Display(Name = "Loan Amount")]

        public double? LoanAmount { get; set; }
        [Display(Name = "Start Date")]

        public DateTime StartDate { get; set; }
        [Display(Name = "Mature Date")]

        public DateTime? EndDate { get; set; }

        public virtual ApplicationUser User { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; }
    }
}
