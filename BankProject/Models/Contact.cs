using BankProject.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BankProject.Models
{
    public class Contacts
    {
        [Key]
        public int Id { get; set; }
        public string OwnerUserId { get; set; } // The user who added the contact
        public virtual ApplicationUser Owner { get; set; }
        public string ContactUserId { get; set; } // The ID of the contact user
        public virtual ApplicationUser Contact { get; set; }
        public string ContactName { get; set; } // The name stored by the owner
        public string ContactEmail { get; set; } // Email of the contact
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

   

    }

}
