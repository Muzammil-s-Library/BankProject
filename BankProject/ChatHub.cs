using BankProject.Data;
using BankProject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;

namespace BankProject
{
    public class ChatHub : Hub
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ChatHub(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task SendMessage(string receiverId, string message)
        {
            var senderId = _userManager.GetUserId(Context.User);
            if (senderId == null) return;

            await Clients.User(receiverId).SendAsync("ReceiveMessage", senderId, message);
        }

      
    }
}
