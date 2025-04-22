using Microsoft.AspNetCore.Authorization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using BankProject.Data;

namespace BankProject.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterConfirmationModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _sender;

        public RegisterConfirmationModel(UserManager<ApplicationUser> userManager, IEmailSender sender)
        {
            _userManager = userManager;
            _sender = sender;
        }



        public async Task<IActionResult> OnGetAsync(string email, string returnUrl = null)
        {
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToPage("/Index");
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound($"Unable to load user with email '{email}'.");
            }
            ViewData["Email"] = email;
            return Page();
        }
        public async Task<IActionResult> OnPostAsync(string otp,string email)
        {
            if (string.IsNullOrEmpty(otp))
            {
                ModelState.AddModelError(string.Empty, "OTP is required.");
                return Page();
            }

            // Assuming Session["OTP"] holds the generated OTP
            var sessionOtp = HttpContext.Session.GetString("OtpCode");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound($"Unable to load user.");
            }

            // OTP matches; proceed to confirm the user's email manually
            if (string.Equals(sessionOtp, otp))
            {
                // Directly update the EmailConfirmed property
                user.EmailConfirmed = true;

                // Save the changes to the user
                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    ModelState.AddModelError(string.Empty, "Error confirming email.");
                    return Page();
                }

                TempData["success"] = "Email verified successfully!";
                return RedirectToPage("/Account/Login");
            }
            else
            {
                TempData["error"] = "Invalid OTP";
                return Page();
            }

        }
    }
}
