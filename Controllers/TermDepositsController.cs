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

    public class TermDepositsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TermDepositsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: TermDeposits
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.TermDeposit.Where(c => c.User.Email == User.Identity.Name);
            return View(await applicationDbContext.ToListAsync());
        }


        //GET
        public async Task<IActionResult> WithDraw(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var account = await _context.TermDeposit.FindAsync(id);
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
            var account = await _context.TermDeposit.FindAsync(id);
            if (account != null && account.EndDate!=null)
            {

                if (DateTime.Now >= account.EndDate)
                {
                    if (amount<=account.Balance)
                    {
                        account.Balance -= amount;
                        _context.Update(account);
                        await _context.SaveChangesAsync();

                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("Balance", "Can't withdraw more amount than Balance");
                        return View(account);
                    }
                }
                else
                {
                    ModelState.AddModelError("EndDate", "Can not withdraw before maturity date.");
                    return View(account);
                }

            }
            return View();
        }

        //GET
        public async Task<IActionResult> Transfer(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var account = await _context.TermDeposit.FindAsync(id);
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
            var account1 = await _context.TermDeposit.FindAsync(id);
            Account account2 = await _context.Account.FindAsync(id2);



            if (account1 != null && account2 != null)
            {
                if (DateTime.Now>=account1.EndDate  )
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
                else
                {
                    ModelState.AddModelError("EndDate", "Can not withdraw before maturity date.");
                    return View(account1);
                }
            }
            ModelState.AddModelError("AccountId", "Invalid Account  please try again");

            return View(account1);
        }



        // GET: TermDeposits/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TermDeposits/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AccountId,UserId,Type,InterestRate,OverDraft,Balance,LoanAmount,StartDate,EndDate")] TermDeposit termDeposit)
        {
            if (termDeposit.EndDate < DateTime.Now)
            {
                ModelState.AddModelError("StartDate", "Can not choose  maturity date before today.");
                return View();

            }
            else
            {
                termDeposit.UserId = _context.Users.Where(a => a.Email == User.Identity.Name).FirstOrDefault().Id;
                termDeposit.Type = "termDeposit";
                termDeposit.StartDate = DateTime.Now.Date;
                termDeposit.InterestRate = .1;
                termDeposit.Balance *= (double)termDeposit.InterestRate;
                if (ModelState.IsValid)
                {
                    _context.Add(termDeposit);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                return View(termDeposit);
            }
        }

        // GET: TermDeposits/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var termDeposit = await _context.TermDeposit.FindAsync(id);
            if (termDeposit == null)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(_context.Set<ApplicationUser>(), "Id", "Id", termDeposit.UserId);
            return View(termDeposit);
        }

        // POST: TermDeposits/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AccountId,UserId,Type,InterestRate,OverDraft,Balance,LoanAmount,StartDate,EndDate")] TermDeposit termDeposit)
        {
            if (id != termDeposit.AccountId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(termDeposit);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TermDepositExists(termDeposit.AccountId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Set<ApplicationUser>(), "Id", "Id", termDeposit.UserId);
            return View(termDeposit);
        }

        // GET: TermDeposits/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var termDeposit = await _context.TermDeposit
                .Include(t => t.User)
                .FirstOrDefaultAsync(m => m.AccountId == id);
            if (termDeposit == null)
            {
                return NotFound();
            }

            return View(termDeposit);
        }

        // POST: TermDeposits/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var termDeposit = await _context.TermDeposit.FindAsync(id);
            _context.TermDeposit.Remove(termDeposit);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TermDepositExists(int id)
        {
            return _context.TermDeposit.Any(e => e.AccountId == id);
        }
    }
}
