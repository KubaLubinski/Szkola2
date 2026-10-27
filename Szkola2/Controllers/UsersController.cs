using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Szkola2.Models.ViewModels;
using Szkola2.Models;
using Microsoft.EntityFrameworkCore;
using Szkola2.Data;
using Szkola2.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Szkola2.Controllers
{
    [Authorize(Roles = "admin")]
    public class UsersController : Controller
    {
        private readonly ILogger<UsersController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IDemoContext _demo;

        public UsersController(ILogger<UsersController> logger, ApplicationDbContext context,
            UserManager<AppUser> userManager, IDemoContext demo)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
            _demo = demo;
        }

        public async Task<IActionResult> UsersList()
        {
            // konta demo widoczne tylko dla ich właściciela
            var myDemoId = _demo.UserId;

            var query = _context.AppUsers!.AsQueryable();
            query = myDemoId == null
                ? query.Where(u => EF.Property<bool?>(u, nameof(AppUser.IsDemo)) != true)
                : query.Where(u => EF.Property<bool?>(u, nameof(AppUser.IsDemo)) != true || u.Id == myDemoId);

            var users = await (from user in query
                               join userRole in _context.UserRoles on user.Id equals userRole.UserId into userRoles
                               from userRole in userRoles.DefaultIfEmpty()
                               join role in _context.Roles on userRole.RoleId equals role.Id into roles
                               from role in roles.DefaultIfEmpty()
                               select new UserViewModel
                               {
                                   Id = user.Id,
                                   Email = user.Email,
                                   FirstName = user.FirstName,
                                   LastName = user.LastName,
                                   IsActive = user.IsActive,
                                   Role = role == null ? null : role.Name
                               }).ToListAsync();

            // jeden wiersz na użytkownika
            var rows = users
                .GroupBy(u => u.Id)
                .Select(g =>
                {
                    var row = g.First();
                    row.Role = string.Join(", ", g.Select(x => x.Role).Where(r => !string.IsNullOrEmpty(r)).Distinct());
                    return row;
                })
                .ToList();

            if (_demo.IsDemo)
            {
                foreach (var row in rows)
                {
                    if (row.Id == myDemoId)
                    {
                        // krótka etykieta zamiast długiego adresu konta demo
                        row.Email = "Twoje konto testowe";
                        row.Role = "konto testowe";
                    }
                    else
                    {
                        row.Email = DemoLimits.MaskEmail(row.Email);
                    }
                }
            }

            return View(rows);
        }

        [HttpGet]
        public async Task<IActionResult> UserEdit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var myDemoId = _demo.UserId;

            var model = await (from user in _context.AppUsers
                               where user.Id == id && (EF.Property<bool?>(user, nameof(AppUser.IsDemo)) != true || user.Id == myDemoId)
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

            if (model == null)
            {
                return NotFound();
            }

            if (_demo.IsDemo && model.Id != myDemoId)
            {
                model.Email = DemoLimits.MaskEmail(model.Email);
                TempData[DemoMessages.Key] = DemoMessages.ViewOnly;
            }

            var roles = await _context.Roles.Where(r => r.Name != "admin").ToListAsync();
            string UserRole = model.Role;
            ViewData["RoleId"] = new SelectList(roles, "Name", "Name", model.Role);
            ViewBag.UserRole = UserRole;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UserEdit(UserViewModel model)
        {
            if (_demo.IsDemo)
            {
                TempData[DemoMessages.Key] = DemoMessages.ReadOnly;
                return RedirectToAction(nameof(UsersList));
            }

            ModelState.Remove("Role");

            if (!ModelState.IsValid)
            {
                await FillRolesAsync(model);
                return View(model);
            }

            var user = await _context.AppUsers.FindAsync(model.Id);
            if (user == null || user.IsDemo)
            {
                return NotFound();
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            bool wasActive = user.IsActive;
            bool isAdminAccount = currentRoles.Contains("admin");
            bool isSelf = user.Id == User.FindFirstValue(ClaimTypes.NameIdentifier);

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Information = model.Information;
            user.CanAddTexts = model.CanAddTexts;
            // admin nie wyłączy własnego konta
            user.IsActive = isSelf && isAdminAccount ? true : model.IsActive;

            // rola tylko u kont nie-adminów, z listy z formularza
            if (!isAdminAccount && !string.IsNullOrEmpty(model.Role) && !currentRoles.Contains(model.Role))
            {
                var allowed = await _context.Roles.AnyAsync(r => r.Name == model.Role && r.Name != "admin");
                if (!allowed)
                {
                    ModelState.AddModelError(nameof(model.Role), "Wybrana rola nie istnieje.");
                    await FillRolesAsync(model);
                    return View(model);
                }

                // najpierw nowa rola, potem usunięcie starych
                var added = await _userManager.AddToRoleAsync(user, model.Role);
                if (!added.Succeeded)
                {
                    ModelState.AddModelError(nameof(model.Role), "Nie udało się zmienić roli.");
                    await FillRolesAsync(model);
                    return View(model);
                }
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                _logger.LogWarning("Admin {AdminId} zmienił rolę konta {UserId}: {OldRoles} -> {NewRole}",
                    User.FindFirstValue(ClaimTypes.NameIdentifier), user.Id, string.Join(",", currentRoles), model.Role);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Admin {AdminId} zapisał dane konta {UserId}", User.FindFirstValue(ClaimTypes.NameIdentifier), user.Id);
            if (wasActive != user.IsActive)
            {
                _logger.LogWarning("Admin {AdminId} {Action} konto {UserId}",
                    User.FindFirstValue(ClaimTypes.NameIdentifier), user.IsActive ? "włączył" : "wyłączył", user.Id);
            }
            return RedirectToAction(nameof(UsersList));
        }

        private async Task FillRolesAsync(UserViewModel model)
        {
            var roles = await _context.Roles.Where(r => r.Name != "admin").ToListAsync();
            ViewData["RoleId"] = new SelectList(roles, "Name", "Name", model.Role);
            ViewBag.UserRole = model.Role;
        }
    }
}
