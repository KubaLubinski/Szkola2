using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Szkola2.Models.ViewModels;
using Szkola2.Models;
using Microsoft.EntityFrameworkCore;

namespace Szkola2.Controllers
{
    public class UsersController : Controller
    {
        private readonly UserManager<AppUser> _userManager;

        public UsersController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users
                .Select(u => new UserViewModel
                {
                    Id = u.Id,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    IsActive = u.IsActive
                })
                .ToListAsync();

            return View(users);
        }
    }
}
