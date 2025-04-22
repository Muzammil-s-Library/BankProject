namespace BankProject.Models
{
    public class Bank
    {
        public int BankId { get; set; } // Primary Key
        public string Name { get; set; } // Bank Name


        // Navigation property
        public ICollection<Account> Accounts { get; set; }
    }
}
