using BankProject.Data;
using Microsoft.AspNetCore.Identity;

namespace BankProject.Models
{
    public class Account
    {
        public int AccountId { get; set; }
        public string AccountNumber { get; set; }
        public string AccountType { get; set; } // e.g., Savings, Current
        public decimal Balance { get; set; }

        public string UserId { get; set; } // Foreign Key to Identity User
        public ApplicationUser User { get; set; } // Navigation property to Identity User

        public int BankId { get; set; } // Foreign Key to Bank
        public Bank Bank { get; set; } // Navigation property to Bank
        public string PhoneNumber { get; set; } // Phone number for verification
        public bool PhoneNumberConfirmed { get; set; } // Indicates whether the phone number is confirmed
        public string TransactionPassword { get; set; } // Password for transactions

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public ICollection<ChequeBookRequest> ChequeBookRequests { get; set; }
    }
}
