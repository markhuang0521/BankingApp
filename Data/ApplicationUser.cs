using Bank.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bank.Data
{
    public class ApplicationUser : IdentityUser
    {
        public virtual ICollection<Account> MyAccounts { get; set; }
    }
}
