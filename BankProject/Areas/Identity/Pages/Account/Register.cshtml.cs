using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BankProject.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using BankProject.Services;
using Microsoft.AspNetCore.Http; // Required for session

namespace BankProject.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "Full Name")]
            public string FullName { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email Address")]
            public string Email { get; set; }

            [Required]
            [Display(Name = "Username")]
            public string Username { get; set; }

            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Confirm Password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        private string GenerateOtpCode()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var existingUser = await _userManager.FindByEmailAsync(Input.Email);

            if (existingUser != null)
            {
                if (!existingUser.EmailConfirmed)
                {
                    // User exists but email is not verified, delete previous OTP-related data if any
                    await _userManager.DeleteAsync(existingUser);

                    // Create a new user entry
                    var newUser = new ApplicationUser
                    {
                        UserName = Input.Username,
                        Email = Input.Email,
                        FullName = Input.FullName
                    };

                    var result = await _userManager.CreateAsync(newUser, Input.Password);

                    if (result.Succeeded)
                    {
                        var otpCode = GenerateOtpCode();
                        HttpContext.Session.SetString("OtpCode", otpCode);

                        bool emailSent = await SendEmail.SendTemplateEmail(newUser.Email, newUser.UserName, otpCode);

                        if (emailSent)
                        {
                            _logger.LogInformation("OTP sent. Please check your email.");
                            return RedirectToPage("/Account/RegisterConfirmation", new { email = newUser.Email });
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Failed to send OTP. Please try again.");
                            return Page();
                        }
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
                else
                {
                    // Email is already verified, redirect to home or login
                    ModelState.AddModelError(string.Empty, "This email is already registered and verified ! Please Login.");
                    return Page();

                }
            }
            else
            {
                // User does not exist, create a new account and send OTP
                var user = new ApplicationUser
                {
                    UserName = Input.Username,
                    Email = Input.Email,
                    FullName = Input.FullName
                };

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    var otpCode = GenerateOtpCode();
                    HttpContext.Session.SetString("OtpCode", otpCode);

                    bool emailSent = await SendEmail.SendTemplateEmail(user.Email, user.UserName, otpCode);

                    if (emailSent)
                    {
                        _logger.LogInformation("OTP sent. Please check your email.");
                        return RedirectToPage("/Account/RegisterConfirmation", new { email = user.Email });
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Failed to send OTP. Please try again.");
                        return Page();
                    }
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return Page();
        }
    }
}
