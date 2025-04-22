using BankProject.Data;
using BankProject.Models;
using BankProject.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NuGet.Versioning;
using System.Linq;
using System.Threading.Tasks;

namespace BankProject.Areas.Client.Controllers
{
    [Area("Client")]
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public IActionResult Account()
        {
            var userId = User.Claims.First().Value; // Get the logged-in user's ID
            List<Account> accounts = _context.Accounts
        .Where(a => a.UserId == userId) // Filter by logged-in user
        .Include(a => a.Bank) // Include the related Bank entity
        .ToList();

            return View(accounts); // Pass accounts to the view
        }

        public IActionResult Index()
        {
            ViewBag.Banks = _context.Banks
                .Select(b => new SelectListItem
                {
                    Value = b.BankId.ToString(), // Use bank name as value
                    Text = b.Name   // Display bank name
                })
                .ToList();

            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Create(Account account)
        {
            // Check if the account number already exists in the same bank
            var existingAccount = _context.Accounts
                .FirstOrDefault(a => a.PhoneNumber == account.PhoneNumber && a.BankId == account.BankId);

            if (existingAccount != null)
            {
                TempData["error"] = "An account with this number already exists for this bank.";
                ModelState.AddModelError("", "You already have an account with this bank with the same Number.");
                // Reload the banks list for the dropdown
                ViewBag.Banks = new SelectList(_context.Banks, "BankId", "Name");
                return View("Index", account); // Return the view with the error message
            }

            if (account.BankId != null)
            {
                account.AccountNumber = GenerateAccountNumber(account.BankId);
            }

            account.UserId = User.Claims.First().Value;

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();
            return RedirectToAction("Account");

            // If model validation fails, reload the banks list and return the view
            ViewBag.Banks = new SelectList(_context.Banks, "BankId", "Name");
            return View(account);
        }


        // Function to generate account number based on bank name
        private string GenerateAccountNumber(int bankId)
        {
            var bank =  _context.Banks.FirstOrDefault(v=>v.BankId==bankId);
            var bankname = bank.Name;
            string prefix = bankname switch
            {
                "Meezan" => "PKMZN",
                "UBL" => "PKUBL",
                "ABL" => "PKABL",
                _ => "PKGEN" // Default prefix for other banks
            };

            string randomNumber = new Random().Next(100000000, 999999999).ToString();
            return $"{prefix}{randomNumber}";
        }
        [HttpGet]
        public async Task<IActionResult> Accountdetails(int id)
        {
            var account = await _context.Accounts
                .Include(a => a.Bank) // Include related Bank data   
                .FirstOrDefaultAsync(a => a.AccountId == id);

            if (account == null) return NotFound();

            var transactions = await _context.Transfers
                .Where(a => a.SenderAccountNumber == account.AccountNumber)
                .ToListAsync();
            var recivetransactions = await _context.Transfers
              .Where(a => a.ReceiverAccountNumber == account.AccountNumber && a.Status == "Approved")
              .ToListAsync(); 

            var viewModel = new AccountDetailsViewModel
            {
                Account = account,
                Transactions = transactions,
                recTransactions= recivetransactions
            };

            return View(viewModel);
        }
        [HttpGet]
        public async Task<IActionResult> TransactionHistory(int id)
        {
            var account = await _context.Accounts
                .Include(a => a.Bank) // Include related Bank data   
                .FirstOrDefaultAsync(a => a.AccountId == id);

            if (account == null) return NotFound();

            var transactions = await _context.Transfers
                           .Where(a => a.SenderAccountNumber == account.AccountNumber)
                           .ToListAsync();
            var recivetransactions = await _context.Transfers
              .Where(a => a.ReceiverAccountNumber == account.AccountNumber && a.Status == "Approved")
              .ToListAsync();
            var viewModel = new AccountDetailsViewModel
            {
                Account = account,
                Transactions = transactions,
                recTransactions= recivetransactions
            };

            return View(viewModel);
        }




        [HttpGet]
        public IActionResult SendMoney(int Id)
        {
            ViewBag.SenderId = Id;

            ViewBag.Banks = _context.Banks
                .Select(b => new SelectListItem
                {
                    Value = b.BankId.ToString(),
                    Text = b.Name
                })
                .ToList();

            return View();
        }


        [HttpPost]
        public async Task<IActionResult> ConfirmMoney(SendMoneyViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Reload banks list in case of validation errors
                ViewBag.Banks = _context.Banks
                    .Select(b => new SelectListItem
                    {
                        Value = b.BankId.ToString(),
                        Text = b.Name
                    })
                    .ToList();
                return View("SendMoney", model);
            }
            var id = model.Senderid;
            var senderaccount = await _context.Accounts.FirstOrDefaultAsync(a => a.AccountId ==id);
            var sendinguser = await _context.Users.FirstOrDefaultAsync(a => a.Id == senderaccount.UserId);
            var senderBank = await _context.Banks.FirstOrDefaultAsync(a => a.BankId ==senderaccount.BankId);
            



            var bank = await _context.Banks.FindAsync(model.BankId);

            if (bank == null)
            {
                ModelState.AddModelError("", "Selected bank is not valid.");
                return RedirectToAction("SendMoney");
            }

            // Get the account associated with the provided account number
            var Reciveraccount = await _context.Accounts
                .Include(a => a.User) // Ensure the User navigation property is loaded
                .FirstOrDefaultAsync(a => a.AccountNumber == model.AccountNumber);

            if (Reciveraccount == null)
            {
                ModelState.AddModelError("", "Account not found.");
                return RedirectToAction("SendMoney");
            }

            // Check if the account's bank matches the selected bank
            if (Reciveraccount.BankId != model.BankId)
            {
                ModelState.AddModelError("", "The selected account does not belong to the selected bank.");
                TempData["error"] = "The selected account does not belong to the selected bank";
                return RedirectToAction("SendMoney");
            }

            // Get the user's name from the linked Identity user
            string ReciveruUserName = Reciveraccount.User?.UserName ?? "Unknown User";

            // Pass the details to the confirmation view
            var confirmModel = new ConfirmMoneyViewModel
            {
                //sender
                senderName = sendinguser.UserName,
                SenderAccountNumber = senderaccount.AccountNumber,
                SenderBankName=senderBank.Name,
                //reciver

                ReceiverAccountNumber = model.AccountNumber,
                ReceiverBankName = bank.Name,
                ReciverName = ReciveruUserName, // Assuming you have a UserName property in ConfirmMoneyViewModel
                
                Amount = model.Amount,
             
                Description = model.Description
            };

            return View("ConfirmMoney", confirmModel);
        }




        [HttpPost]
        public async Task<IActionResult> FinalizeMoneyTransfer(ConfirmMoneyViewModel model)
        {
            var md = model;
            // Validate transaction password
            var senderAccount = await _context.Accounts
                .FirstOrDefaultAsync(a => a.AccountNumber == model.SenderAccountNumber);

            if (senderAccount == null)
            {
                TempData["error"]="Sender account not found.";
                return RedirectToAction("SendMoney", new { id = senderAccount.AccountId });

            }

            if (senderAccount.TransactionPassword != model.TransactionPassword)
            {
                TempData["error"]="Invalid transaction password.";
                return RedirectToAction("SendMoney", new { id = senderAccount.AccountId });

            }

            // Find the receiver's account
            var receiverAccount = await _context.Accounts
                .FirstOrDefaultAsync(a => a.AccountNumber == model.ReceiverAccountNumber);

            if (receiverAccount == null)
            {
                TempData["error"]="Receiver account not found.";
                return RedirectToAction("SendMoney", new { id = senderAccount.AccountId });
            }

            // Ensure sender has enough balance
            if (senderAccount.Balance < model.Amount)
            {
                TempData["error"] = "Insufficient balance.";
                return RedirectToAction("SendMoney", new { id = senderAccount.AccountId });
            }

            // Deduct amount from sender
            senderAccount.Balance -= model.Amount;
            _context.Accounts.Update(senderAccount);
            // Add record to Transfer table (Admin will approve later)
            var transferRecord = new Transfer
            {
                SenderAccountNumber = senderAccount.AccountNumber,
                ReceiverAccountNumber = receiverAccount.AccountNumber,
                senderName=model.senderName,
                ReciverName=model.ReciverName,
                Amount = model.Amount,
                Description = model.Description,
                Status = "Not Approved", // Admin will approve this later
                SentAt = DateTime.UtcNow
            };

            _context.Transfers.Add(transferRecord);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Money transfer request submitted. Awaiting admin approval.";
            return RedirectToAction("TransactionSlip",md);
        }
        public IActionResult TransactionSlip(ConfirmMoneyViewModel model) {
            return View(model);       
        }

        [HttpGet]
        public async Task<IActionResult> Requestch(int id)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.AccountId == id);

            if (account == null)
            {
                return NotFound();
            }

            var latestRequest = await _context.ChequeBookRequests
                .Where(r => r.AccountId == id)
                .OrderByDescending(r => r.CreatedAt)
                .FirstOrDefaultAsync();

            var requestModel = new Request
            {
                AccountNumber = account.AccountNumber,
                Id = account.AccountId,
                DeliveryAddress = null,
                NumberOfLeaves = null
            };

            // Pass the latest request status if available
            ViewBag.Status = latestRequest?.Status ?? "NoRequest";

            return View(requestModel);
        }

        [HttpPost]
        public async Task<IActionResult> RequestCh(Request sho)
        {
            // Validation: Check if any required field is null or empty
            if (string.IsNullOrWhiteSpace(sho.AccountNumber) ||
                string.IsNullOrWhiteSpace(sho.DeliveryAddress) ||
                sho.NumberOfLeaves == null)
            {
                TempData["error"] = "All fields are required. Please fill in all details.";
                return View(sho); // Return to the same page with entered values
            }

            var acc = new ChequeBookRequest
            {
                AccountId = sho.Id,
                RequestedAccountNumber = sho.AccountNumber,
                DeliveryAddress = sho.DeliveryAddress,
                NumberOfLeaves = sho.NumberOfLeaves,
                Status = "Pending"
            };

            await _context.ChequeBookRequests.AddAsync(acc);
            await _context.SaveChangesAsync();

            TempData["success"] = "Request Submitted";
            return RedirectToAction("Accountdetails", new { id = acc.AccountId });
        }



        [HttpPost]
        public async Task<IActionResult> CancelTransaction(int transactionId)
        {
            var transaction = await _context.Transfers.FindAsync(transactionId);

            if (transaction == null)
            {
                TempData["error"] = "Transaction not found.";
                return RedirectToAction("TransactionHistory", new { id = transactionId });
            }

            if (transaction.Status != "Not Approved")
            {
                TempData["error"] = "Only pending transactions can be canceled.";
                return RedirectToAction("TransactionHistory", new { id = transactionId });
            }

            // Refund the sender's account balance
            var senderAccount = await _context.Accounts
                .FirstOrDefaultAsync(a => a.AccountNumber == transaction.SenderAccountNumber);

            if (senderAccount != null)
            {
                senderAccount.Balance += transaction.Amount;
                _context.Accounts.Update(senderAccount);
            }

            // Remove the transaction
            _context.Transfers.Remove(transaction);
            await _context.SaveChangesAsync();

            TempData["success"] = "Transaction has been canceled successfully.";
            return RedirectToAction("TransactionHistory", new { id = senderAccount.AccountId });
        }



    }
}
