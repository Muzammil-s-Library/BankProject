namespace BankProject.Models
{
    public class Transfer
    {
        public int TransferId { get; set; } // Primary Key

        // Sender details linked via AccountNumber
        public string SenderAccountNumber { get; set; } // Foreign Key to Sender Account via AccountNumber
        public Account SenderAccount { get; set; } // Navigation property for Sender Account

        // Receiver details linked via AccountNumber
        public string senderName { get; set; }
        public string ReciverName { get; set; }
        public string ReceiverAccountNumber { get; set; } // Foreign Key to Receiver Account via AccountNumber
        public Account ReceiverAccount { get; set; } // Navigation property for Receiver Account

        // Other transfer details
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string Status { get; set; } = "Not Approved"; // Default status
        public DateTime SentAt { get; set; } = DateTime.UtcNow; // Default to current time
    }
}
