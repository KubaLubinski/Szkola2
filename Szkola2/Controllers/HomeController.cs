using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Szkola2.Data;
using Szkola2.Models;
using Szkola2.Models.ViewModels;

namespace Szkola2.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> IndexAsync()
        {
            var latestTexts = await _context.Texts
                                .Include(t => t.Category)
                                .Include(t => t.User)
                                .Where(t => t.IsActive)
                                .OrderByDescending(t => t.DateAdded)
                                .Take(6)
                                .ToListAsync();

            var advertisingPhotos = await _context.AdvertisingPhotos.ToListAsync();


            var model = new HomeViewModel
            {
                LatestTexts = latestTexts,
                AdvertisingPhotos = advertisingPhotos
            };

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult Accessibility()
        {
            return View();
        }
        public IActionResult Contact()
        {
            return View();
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}