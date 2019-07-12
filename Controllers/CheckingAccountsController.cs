using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Bank.Data;
using Bank.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace Bank.Controllers
{

    [Authorize]
    public class CheckingAccountsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CheckingAccountsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: CheckingAccounts
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.CheckingAccount.Where(c =>c.User.Email==User.Identity.Name);
            return View(await applicationDbContext.ToListAsync());
        }

        //GET
        public async Task<IActionResult> Deposit(int? id)
        {
            if (id == null)
            {
                return View();
            }

            var account = await _context.CheckingAccount.FindAsync(id);
            if (account == null)
            {
                return View();
            }
            return View(account);
        }
        //Post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deposit(int id, double amount)
        {
            var account = await _context.CheckingAccount.FindAsync(id);
            if (account != null)
            {
                try
                {
                    account.Balance += amount;
                    _context.Update(account);

                    var trans = new Transaction
                    {
                        Type="deposit",
                        Amount=amount,
                        AccountId=id,
                        transTime=DateTime.Now

                    };
                    _context.Update(trans);

                    await _context.SaveChangesAsync();
                }
                catch (Exception)
                {

                    throw;
                }
                return RedirectToAction("Index");

            }
            return View();

        }
        //GET
        public async Task<IActionResult> WithDraw(int? id)
        {
            if (id == null)
            {
                return View();
            }

            var account = await _context.CheckingAccount.FindAsync(id);
            if (account == null)
            {
                return View();
            }
            return View(account);
        }
        //Post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> WithDraw(int? id, double amount)
        {
            var account = await _context.CheckingAccount.FindAsync(id);
            if (account != null)
            {
                if (amount<=account.Balance)
                {
                    account.Balance -= amount;
                    _context.Update(account);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    ModelState.AddModelError("Balance", "Can't withdraw more amount than Balance");
                    return View(account);
                }
             

                return RedirectToAction(nameof(Index));
            }
            return View();
        }

        //GET
        public async Task<IActionResult> Transfer(int? id)
        {
            if (id == null)
            {
                return View();
            }

            var account = await _context.CheckingAccount.FindAsync(id);
            if (account == null)
            {
                return View();
            }
            return View(account);
        }
        //Post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Transfer(int? id, int id2, double amount)
        {
            var account1 = await _context.CheckingAccount.FindAsync(id);

            Account account2 = await _context.Account.FindAsync(id2);



            if (account1 != null && account2 != null)
            {
                if (amount <= account1.Balance)
                {
                    account1.Balance -= amount;
                    _context.Update(account1);
                    account2.Balance += amount;
                    _context.Update(account2);

                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("Balance", "Can't withdraw more amount than Balance");
                    return View(account1);
                }
            }
            ModelState.AddModelError("AccountId", "Invalid Account  please try again");

            return View(account1);
        }




        // GET: CheckingAccounts/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: CheckingAccounts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AccountId,UserId,Type,InterestRate,OverDraft,Balance,LoanAmount,StartDate,EndDate")] CheckingAccount checkingAccount)
        {
            checkingAccount.UserId = _context.Users.Where(a => a.Email == User.Identity.Name).FirstOrDefault().Id;
            checkingAccount.Type = "checking";
            checkingAccount.StartDate = DateTime.Now;
            checkingAccount.InterestRate = .05;
            checkingAccount.OverDraft = 0;
            if (ModelState.IsValid)
            {
                _context.Add(checkingAccount);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(checkingAccount);
        }





        // GET: CheckingAccounts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return View();
            }

            var checkingAccount = await _context.CheckingAccount
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.AccountId == id);
            if (checkingAccount == null)
            {
                return View();
            }

            return View(checkingAccount);
        }

        // POST: CheckingAccounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var checkingAccount = await _context.CheckingAccount.FindAsync(id);
            _context.CheckingAccount.Remove(checkingAccount);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CheckingAccountExists(int id)
        {
            return _context.CheckingAccount.Any(e => e.AccountId == id);
        }
    }
}
