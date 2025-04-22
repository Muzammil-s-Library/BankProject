using BankProject.Data;
using BankProject.Data.ViewModels;
using BankProject.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace BankProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Fetch all users
        public async Task<IActionResult> Index()
        {
            var totalUsers = await _context.Users.CountAsync();
            var totalAccounts = await _context.Accounts.CountAsync();

            var totalRequests = await _context.ChequeBookRequests.CountAsync();

            var model = new DashboardViewModel
            {
                TotalUsers = await _context.Users.CountAsync(),
                TotalAccounts = await _context.Accounts.CountAsync(),
                TotalTransactions = await _context.Transfers.CountAsync(),
                TotalRequests = await _context.ChequeBookRequests.CountAsync()
            };

            return View(model);
        }

        // Register action (you can implement your registration logic here)
        public IActionResult Register()
        {
            return View();
        }

        // Fetch all transactions
       
        // Fetch all users with accounts and associated data
        public async Task<IActionResult> Users()
        {
            List<ApplicationUser> users = await _context.Users.Cast<ApplicationUser>().ToListAsync();
            
            return View(users);
        }

        // Fetch all cheque book requests
        public async Task<IActionResult> Requests()
        {
            var requests = await _context.ChequeBookRequests
                .Include(r => r.Account) // Include related Account
                .ThenInclude(a => a.User) // Include related User
                .ToListAsync();
            return View(requests);
        }
        public async Task<IActionResult> Banks()
        {
            List<Bank> banks = await _context.Banks
            
                .ToListAsync();

            return View(banks);
        }
        public IActionResult CreateBank() {
            return View();
        }
 
        [HttpPost]
        public async Task<IActionResult> CreateBank(Bank bank)
        {
           
                _context.Banks.Add(bank);
                await _context.SaveChangesAsync();
                return RedirectToAction("Banks");
         
         
        }

        [HttpPost]
        public async Task<IActionResult> DeleteBank(int id)
        {
            var bank = await _context.Banks.FindAsync(id);
            if (bank == null)
            {
                return NotFound();
            }

            _context.Banks.Remove(bank);
            await _context.SaveChangesAsync();

            return RedirectToAction("Banks");
        }
        public async Task<IActionResult> EditBank(int id)
        {
            var bank = await _context.Banks.FindAsync(id);
            if (bank == null)
            {
                return NotFound();
            }
            return View(bank);
        }
        [HttpPost]
        public async Task<IActionResult> EditBank(int id, Bank bank)
        {
            if (id != bank.BankId)
            {
                return BadRequest();
            }

            var existingBank = await _context.Banks.FindAsync(id);
            if (existingBank == null)
            {
                return NotFound();
            }

            existingBank.Name = bank.Name; // Update only the name
            await _context.SaveChangesAsync();

            return RedirectToAction("Banks");
        }


        // Fetch all accounts
        public async Task<IActionResult> Account()
        {
            List<Account> accounts = await _context.Accounts
                .Include(a => a.User) // Include related user for each account
                .Include(a => a.Bank) // Include related bank for each account
                .ToListAsync();
            return View(accounts);
        }

        // Approve cheque book request
        [HttpPost]
        public async Task<IActionResult> ApproveChequeBookRequest(int requestId)
        {
            var request = await _context.ChequeBookRequests
                .FirstOrDefaultAsync(r => r.RequestId == requestId);

            if (request != null)
            {
                request.Status = "Approved"; // Set status to Approved
                request.UpdatedAt = DateTime.UtcNow;

                _context.Update(request);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Requests");
        }

        // Reject cheque book request
        [HttpPost]
        public async Task<IActionResult> RejectChequeBookRequest(int requestId)
        {
            var request = await _context.ChequeBookRequests
                .FirstOrDefaultAsync(r => r.RequestId == requestId);

            if (request != null)
            {
                request.Status = "Rejected"; // Set status to Rejected
                request.UpdatedAt = DateTime.UtcNow;

                _context.Update(request);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Requests");
        }
        [HttpGet]
        public async Task<IActionResult> Transactionssss()
        {
            List<Transfer> transactions = await _context.Transfers.ToListAsync();
            return View(transactions);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveTransaction(int transactionId)
        {
            var transaction = await _context.Transfers.FindAsync(transactionId);

            if (transaction == null)
            {
                TempData["error"] = "Transaction not found.";
                return RedirectToAction("Transactions");
            }

            if (transaction.Status != "Not Approved")
            {
                TempData["error"] = "Transaction has already been processed.";
                return RedirectToAction("Transactions");
            }

            // Deduct money from sender's account

            var receiverAccount = await _context.Accounts.FirstOrDefaultAsync(a => a.AccountNumber == transaction.ReceiverAccountNumber);

            receiverAccount.Balance += transaction.Amount;

            transaction.Status = "Approved";


            _context.Update(receiverAccount);
            _context.Update(transaction);
            await _context.SaveChangesAsync();

            TempData["success"] = "Transaction approved successfully.";
            return RedirectToAction("Transactionssss");
        }




    }
}
