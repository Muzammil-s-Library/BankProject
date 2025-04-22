using BankProject.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BankProject.Models
{
    public class Message
    {
        [Key]
        public int Id { get; set; }
        public string SenderId { get; set; } // Sender user ID
        public virtual ApplicationUser Sender { get; set; }
        public string ReceiverId { get; set; } // Receiver user ID
        public virtual ApplicationUser Receiver { get; set; }
        public string Content { get; set; } // Message content
        public bool IsRead { get; set; } = false; // Message read status
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        


    }

}
