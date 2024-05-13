using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Szkola2.Models.ViewModels;
using Szkola2.Models;
using Microsoft.EntityFrameworkCore;
using Szkola2.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using static System.Net.Mime.MediaTypeNames;

namespace Szkola2.Controllers
{
    public class UsersController : Controller
    {
        private readonly ILogger<UsersController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public UsersController(ILogger<UsersController> logger, ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }
        public async Task<IActionResult> UsersList()
        {
            var users = await (from user in _context.AppUsers
                               join userRole in _context.UserRoles on user.Id equals userRole.UserId
                               join role in _context.Roles on userRole.RoleId equals role.Id
                               select new UserViewModel
                               {
                                   Id = user.Id,
                                   Email = user.Email,
                                   FirstName = user.FirstName,
                                   LastName = user.LastName,
                                   IsActive = user.IsActive,
                                   Role = role.Name
                               }).ToListAsync();

            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> UserEdit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var model = await (from user in _context.AppUsers
                              where user.Id == id
                              join userRole in _context.UserRoles on user.Id equals userRole.UserId
                              join role in _context.Roles on userRole.RoleId equals role.Id
                              select new UserViewModel
                              {
                                  Id = user.Id,
                                  Email = user.Email,
                                  FirstName = user.FirstName,
                                  LastName = user.LastName,
                                  Information = user.Information,
                                  IsActive = user.IsActive,
                                  CanAddTexts = user.CanAddTexts,
                                  Role = role.Name
                              }).FirstOrDefaultAsync();

            ViewData["RoleId"] = new SelectList(_context.Roles, "Name", "Name", model.Role);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UserEdit(UserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.AppUsers.FindAsync(model.Id);
            if (user != null)
            {
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Information = model.Information;
                user.IsActive = model.IsActive;
                user.CanAddTexts = model.CanAddTexts;

                var roleResult = await _userManager.AddToRoleAsync(user, model.Role);

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(UsersList));
            }

            return View(model);
        }
    }
}
