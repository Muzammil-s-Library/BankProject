using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace BankProject.Data
{
    // Add profile data for application users by adding properties to the ApplicaitonUser class
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }

    }

}
