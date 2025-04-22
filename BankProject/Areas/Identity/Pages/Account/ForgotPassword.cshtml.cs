using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using BankProject.Data;
using BankProject.Services;

namespace BankProject.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;

        public ForgotPasswordModel(UserManager<ApplicationUser> userManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);

                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "No account found with this email.");
                    return Page();
                }

                if (!await _userManager.IsEmailConfirmedAsync(user))
                {
                    ModelState.AddModelError(string.Empty, "You need to verify your email before resetting the password.");
                    return Page();
                }
                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                resetToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(resetToken));
                var ResetLink = Url.Page(
    "/Account/ResetPassword",  // The Razor Page for resetting the password
    pageHandler: null,         // No specific page handler needed
    values: new { area = "Identity", code = resetToken }, // Parameters added to the URL
    protocol: Request.Scheme   // Ensures the URL uses HTTP or HTTPS
);


                                

                var emailSent = await SendPass.SendTemplateEmail(user.Email, user.UserName, ResetLink);
                if (!emailSent)
                {
                    ModelState.AddModelError(string.Empty, "Failed to send OTP. Please try again.");
                    return Page();
                }
                TempData["success"] = "An email has been sent with a password reset link. Please check your inbox.";
                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            return Page();
        }

    }
}
