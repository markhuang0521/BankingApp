using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Bank.Data;
using Bank.Models;
using Microsoft.AspNetCore.Authorization;

namespace Bank.Controllers
{
    [Authorize]
    public class BusinessAccountsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BusinessAccountsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: BusinessAccounts
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.BusinessAccount.Where(c => c.User.Email == User.Identity.Name);
            return View(await applicationDbContext.ToListAsync());
        }
        //GET
        public async Task<IActionResult> Deposit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var account = await _context.BusinessAccount.FindAsync(id);
            if (account == null)
            {
                return NotFound();
            }
            return View(account);
        }
        //Post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deposit(int id, double amount)
        {
            var account = await _context.BusinessAccount.FindAsync(id);
            if (account != null)
            {
                if (account.OverDraft > 0)
                {
                    if (account.OverDraft > amount)
                    {
                        account.OverDraft -= amount;

                    }
                    else
                    {
                        account.Balance += (amount - (double)account.OverDraft);
                        account.OverDraft = 0;

                    }
                }
                else
                {
                    account.Balance += amount;
                 
                }
                _context.Update(account);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index");

            }
            return NotFound();

        }
        //GET
        public async Task<IActionResult> WithDraw(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var account = await _context.BusinessAccount.FindAsync(id);
            if (account == null)
            {
                return NotFound();
            }
            return View(account);
        }
        //Post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> WithDraw(int? id, double amount)
        {
            var account = await _context.BusinessAccount.FindAsync(id);
            if (account != null)
            {
                if (amount <= account.Balance)
                {
                    account.Balance -= amount;

                }
                else
                {
                    account.OverDraft = amount - account.Balance;
                    account.Balance = 0;

                }
                _context.Update(account);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));

            }
            return NotFound();
        }

        //GET
        public async Task<IActionResult> Transfer(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var account = await _context.BusinessAccount.FindAsync(id);
            if (account == null)
            {
                return NotFound();
            }
            return View(account);
        }
        //Post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Transfer(int? id, int id2, double amount)
        {
            var account1 = await _context.BusinessAccount.FindAsync(id);
            Account account2 = await _context.Account.FindAsync(id2);    


            if (account1 != null && account2 != null)
            {

                    if (amount <= account1.Balance)
                    {
                        account1.Balance -= amount;

                    }
                    else
                    {
                        account1.OverDraft = amount - account1.Balance;
                        account1.Balance = 0;

                    }
                    if (account2.OverDraft > 0)
                    {
                        if (account2.OverDraft > amount)
                        {
                            account2.OverDraft -= amount;

                        }
                        else
                        {
                            account2.Balance += (amount - (double)account2.OverDraft);
                            account2.OverDraft = 0;

                        }
                    }
                    else
                    {
                        account2.Balance += amount;

                    }
                    _context.Update(account1);
                    _context.Update(account2);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));

            }
            ModelState.AddModelError("AccountId", "Invalid Account  please try again");

            return View(account1);
        }




            // GET: BusinessAccounts/Create

            public IActionResult Create()
        {
            return View();
        }

        // POST: BusinessAccounts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AccountId,UserId,Type,InterestRate,OverDraft,Balance,LoanAmount,StartDate,EndDate")] BusinessAccount businessAccount)
        {
            businessAccount.UserId = _context.Users.Where(a => a.Email == User.Identity.Name).FirstOrDefault().Id;
            businessAccount.Type = "business";
            businessAccount.StartDate = DateTime.Now.Date;
            businessAccount.InterestRate = .05;
            businessAccount.OverDraft = 0;


            if (ModelState.IsValid)
            {
                _context.Add(businessAccount);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(businessAccount);
        }

        // GET: BusinessAccounts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var businessAccount = await _context.BusinessAccount
                .Include(b => b.User)
                .FirstOrDefaultAsync(m => m.AccountId == id);
            if (businessAccount == null)
            {
                return NotFound();
            }

            return View(businessAccount);
        }

        // POST: BusinessAccounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var businessAccount = await _context.BusinessAccount.FindAsync(id);
            if (businessAccount.OverDraft<=0)
            {
                _context.BusinessAccount.Remove(businessAccount);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            else
            {
                ModelState.AddModelError("OverDraft", "cannot close account with exisitng overdraft!! ");
                return View(businessAccount);
            };

        }


    }
}
