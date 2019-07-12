using System;
using System.Collections.Generic;
using System.Text;
using Bank.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Bank.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Account> Account { get; set; }
        public DbSet<CheckingAccount> CheckingAccount { get; set; }
        public DbSet<BusinessAccount> BusinessAccount { get; set; }
        public DbSet<TermDeposit> TermDeposit { get; set; }

        public DbSet<Loan> Loan { get; set; }
        public DbSet <Transaction> transactions{ get; set; }



        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Account>()
                .HasOne(a => a.User)
                .WithMany(a => a.MyAccounts)
                .HasForeignKey(k => k.UserId)
                .HasConstraintName("UserId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.Entity<Transaction>()
                .HasOne(a => a.MyAccount)
                .WithMany(a=> a.Transactions)
                .HasForeignKey(k => k.AccountId)
                .HasConstraintName("AccountId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
        }













    }
}
