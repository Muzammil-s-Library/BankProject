using SendGrid;
using SendGrid.Helpers.Mail;
using System.Net.Mail;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity.UI.Services;  // Ensure you have this import for Dictionary

namespace BankProject.Services
{
    public class SendPass : IEmailSender
    {
        private const string ApiKey = ""; // Replace with your API Key
        private const string TemplateId = "d-465bd53573cd43ffb4f126c40fa8e3c0"; // Replace with your Template ID
        private const string FromEmail = "muzammiltanoli817@gmail.com"; // Use the verified sender email

        public static async Task<bool> SendTemplateEmail(string recipientEmail, string recipientName, string ResetLink)
        {
            try
            {
                var client = new SendGridClient(ApiKey);
                var from = new EmailAddress(FromEmail, "Banker");
                var to = new EmailAddress(recipientEmail, recipientName);
                var msg = new SendGridMessage
                {
                    From = from,
                    TemplateId = TemplateId
                };

                // Define the dynamic template data with both username and otp_code
                var dynamicTemplateData = new Dictionary<string, string>
                {
                    { "name", recipientName },   // Pass the recipient's name
                    { "reset-link", ResetLink }      // Pass the OTP code
                };

                msg.AddTo(to);
                msg.SetTemplateData(dynamicTemplateData);

                var response = await client.SendEmailAsync(msg);
                return response.StatusCode == System.Net.HttpStatusCode.Accepted;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
                return false;
            }
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            throw new NotImplementedException();
        }
    }
}
