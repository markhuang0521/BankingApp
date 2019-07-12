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
    public class LoansController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LoansController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Loans
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Loan.Where(c => c.User.Email == User.Identity.Name);
            return View(await applicationDbContext.ToListAsync());
        }


    
        //GET
        public async Task<IActionResult> WithDraw(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var account = await _context.Loan.FindAsync(id);
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
            var account = await _context.Loan.FindAsync(id);
            if (account != null)
            {
                account.Balance -= amount;
                _context.Update(account);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            return NotFound();
        }

        //GET
        public async Task<IActionResult> PayLoan(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var account = await _context.Loan.FindAsync(id);
            if (account == null)
            {
                return NotFound();
            }
            return View(account);
        }
        //Post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PayLoan(int? id, int id2, double amount)
        {
            var account1 = await _context.Loan.FindAsync(id);
            Account account2 = await _context.Account.FindAsync(id2);
   


            if (account2 != null && account2.Type == "termDeposit")
            {
                if (DateTime.Now>=account2.EndDate)
                {
                    if (amount<=account2.Balance)
                    {
                        account1.LoanAmount -= amount;
                        if (account1.LoanAmount<=0)
                        {
                            account1.Balance += (amount - (double)account1.LoanAmount);
                            account1.LoanAmount = 0;
                        }
                        _context.Update(account1);
                        account2.Balance -= amount;
                        _context.Update(account2);

                        await _context.SaveChangesAsync();

                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("Balance", "Can't withdraw more amount than Balance");
                        return View(account1);
                    }
                }else
                {
                    ModelState.AddModelError("EndDate", "Can not withdraw before maturity date.");
                    return View(account1);
                }
                
            } else if (account1 != null && account2 != null )
            {
                if (amount <= account2.Balance)
                {
                    if (amount-account1.LoanAmount >= 0)
                    {
                        account1.Balance += (amount - (double)account1.LoanAmount);
                        account1.LoanAmount = 0;
                    }
                    else
                    {
                        account1.LoanAmount -= amount;

                    }
                    _context.Update(account1);
                    account2.Balance -= amount;
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






        // GET: Loans/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Loans/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AccountId,UserId,Type,InterestRate,OverDraft,Balance,LoanAmount,StartDate,EndDate")] Loan loan)
        {
            loan.UserId = _context.Users.Where(a => a.Email == User.Identity.Name).FirstOrDefault().Id;
            loan.Type = "loan";
            loan.StartDate = DateTime.Now;
            loan.InterestRate = .2;
            loan.OverDraft = 0;
            loan.LoanAmount = (double) loan.Balance * (1+(double) loan.InterestRate);
            if (ModelState.IsValid)
            {
                _context.Add(loan);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Set<ApplicationUser>(), "Id", "Id", loan.UserId);
            return View(loan);
        }


        // GET: Loans/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loan = await _context.Loan
                .Include(l => l.User)
                .FirstOrDefaultAsync(m => m.AccountId == id);
            if (loan == null)
            {
                return NotFound();
            }

            return View(loan);
        }

        // POST: Loans/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var loan = await _context.Loan.FindAsync(id);
            _context.Loan.Remove(loan);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LoanExists(int id)
        {
            return _context.Loan.Any(e => e.AccountId == id);
        }
    }
}
