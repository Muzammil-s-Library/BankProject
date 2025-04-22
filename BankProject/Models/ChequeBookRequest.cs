using System;
using System.ComponentModel.DataAnnotations;

namespace BankProject.Models
{
    public class ChequeBookRequest
    {
        [Key]
        public int RequestId { get; set; } // Primary Key

        public int AccountId { get; set; } // Foreign Key to Accounta
        public Account Account { get; set; } // Navigation property

        public string NumberOfLeaves { get; set; } // Number of leaves in the cheque book

        public string DeliveryAddress { get; set; } // Address where the cheque book will be delivered


        public string RequestedAccountNumber { get; set; } // Requested account number

        public string Status { get; set; } = "Pending"; // Status: Pending, Approved, Rejected

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Request creation time
        public DateTime? UpdatedAt { get; set; } // Last update time
    }
}
