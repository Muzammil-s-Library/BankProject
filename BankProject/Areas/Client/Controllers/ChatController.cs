using BankProject.Data;
using BankProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BankProject.Areas.Client.Controllers
{
    [Area("Client")]
    [Authorize]
    public class ChatController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChatController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var contacts = await _context.Contacts
                .Where(c => c.OwnerUserId == user.Id)
                .ToListAsync();

            return View(contacts);
        }

        [HttpPost]
        public async Task<IActionResult> AddContact(string email, string name)
        {
            var user = await _userManager.GetUserAsync(User);
            var contactUser = await _userManager.FindByEmailAsync(email);

            if (contactUser == null || user.Id == contactUser.Id)
                return BadRequest("Invalid user.");

            var contact = new Contacts
            {
                OwnerUserId = user.Id,
                ContactUserId = contactUser.Id,
                ContactName = name,
                ContactEmail = email
            };

            _context.Contacts.Add(contact);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }


       

    }
}
