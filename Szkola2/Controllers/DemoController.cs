using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Szkola2.Data;
using Szkola2.Infrastructure;
using Szkola2.Models;

namespace Szkola2.Controllers
{
    public class DemoController : Controller
    {
        private static readonly string[] FirstNames = { "Anna", "Piotr", "Katarzyna", "Tomasz", "Ewa", "Marcin", "Joanna", "Paweł", "Monika", "Adam" };
        private static readonly string[] LastNames = { "Kowalska", "Nowak", "Wiśniewski", "Wójcik", "Kamińska", "Lewandowski", "Zielińska", "Szymański", "Dąbrowska", "Kozłowski" };
        private static readonly string[] DemoRoles = { "admin", "dyrektor", "nauczyciel", "uczen", "rodzic" };

        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ApplicationDbContext _db;
        private readonly IMemoryCache _cache;
        private readonly ILogger<DemoController> _logger;

        public DemoController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager,
            ApplicationDbContext db, IMemoryCache cache, ILogger<DemoController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _db = db;
            _cache = cache;
            _logger = logger;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Start()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                TempData[DemoMessages.Key] = "Jesteś już zalogowany. Wyloguj się, aby skorzystać z konta testowego.";
                return RedirectToAction("Index", "Home");
            }

            // limit startów z jednego IP
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var cacheKey = "demo-start:" + ip;
            _cache.TryGetValue(cacheKey, out int starts);
            if (starts >= DemoLimits.MaxStartsPerIpPerHour)
            {
                TempData[DemoMessages.Key] = "Zbyt wiele prób utworzenia konta testowego. Spróbuj ponownie za godzinę.";
                return Redirect("/Identity/Account/Login");
            }

            var now = DateTime.UtcNow;
            var active = await _db.AppUsers!.CountAsync(u => u.IsDemo && u.DemoExpiresAt > now);
            if (active >= DemoLimits.MaxActiveAccounts)
            {
                TempData[DemoMessages.Key] = "Chwilowo jest zbyt wiele aktywnych kont testowych. Spróbuj ponownie za kilka minut.";
                return Redirect("/Identity/Account/Login");
            }

            _cache.Set(cacheKey, starts + 1, TimeSpan.FromHours(1));

            var expires = now.AddHours(DemoLimits.SessionHours);
            var email = $"demo-{Guid.NewGuid():N}@demo.local";
            var password = Convert.ToBase64String(RandomNumberGenerator.GetBytes(24)) + "aA1!";

            var user = new AppUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                LockoutEnabled = false,
                FirstName = FirstNames[RandomNumberGenerator.GetInt32(FirstNames.Length)],
                LastName = LastNames[RandomNumberGenerator.GetInt32(LastNames.Length)],
                Information = "Konto testowe (jednorazowe)",
                IsActive = true,
                CanAddTexts = true,
                IsDemo = true,
                DemoExpiresAt = expires
            };

            var created = await _userManager.CreateAsync(user, password);
            if (!created.Succeeded)
            {
                _logger.LogError("Nie udało się utworzyć konta demo: {Errors}", string.Join("; ", created.Errors.Select(e => e.Description)));
                TempData[DemoMessages.Key] = "Nie udało się utworzyć konta testowego. Spróbuj ponownie.";
                return Redirect("/Identity/Account/Login");
            }

            try
            {
                await _userManager.AddToRolesAsync(user, DemoRoles);
                await _userManager.AddClaimsAsync(user, new[]
                {
                    new Claim(DemoClaims.IsDemo, "1"),
                    new Claim(DemoClaims.Expires, new DateTimeOffset(expires).ToUnixTimeSeconds().ToString())
                });
            }
            catch
            {
                await _userManager.DeleteAsync(user);
                throw;
            }

            await _signInManager.SignInAsync(user, new AuthenticationProperties
            {
                IsPersistent = false,
                ExpiresUtc = expires
            });

            return RedirectToAction("Index", "Home");
        }
    }
}
