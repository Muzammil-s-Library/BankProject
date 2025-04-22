using BankProject.Models;

namespace BankProject.ViewModels
{
    public class AccountDetailsViewModel
    {
        public Account Account { get; set; }
        public List<Transfer> Transactions { get; set; }
        public List<Transfer> recTransactions { get; set; }
    }
}

