using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Szkola2.Data;
using Szkola2.Models;

namespace Szkola2.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(ApplicationDbContext context, ILogger<CategoriesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Categories
        [Authorize(Roles = "admin,dyrektor")]
        public async Task<IActionResult> Index()
        {
              return _context.Categories != null ?
                          View(await _context.Categories.ToListAsync()) :
                          Problem("Entity set 'ApplicationDbContext.Categories'  is null.");
        }

        // GET: Categories/Details/5
        [Authorize(Roles = "admin,dyrektor")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.CategoryId == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // GET: Categories/Create
        [Authorize(Roles = "admin,dyrektor")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Categories/Create
        [Authorize(Roles = "admin,dyrektor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CategoryId,CategoryName,Description,IsActive")] Category category)
        {
            if (ModelState.IsValid)
            {
                if (!CategoryNameExists(category.CategoryName))
                {
                    _context.Add(category);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ViewBag.ErrorMessage = "Kategoria o takiej nazwie już istnieje";
                    return (View("Create"));
                }
            }
            return View(category);
        }

        // GET: Categories/Edit/5
        [Authorize(Roles = "admin,dyrektor")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: Categories/Edit/5
        [Authorize(Roles = "admin,dyrektor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CategoryId,CategoryName,Description,IsActive")] Category category)
        {
            if (id != category.CategoryId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.CategoryId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Categories/Delete/5
        [Authorize(Roles = "admin,dyrektor")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.CategoryId == id);
            if (category == null)
            {
                return NotFound();
            }
            if (TextsInCategory((int)id))
            {
                ViewBag.DeleteMessage = "Nie można usunąć wybranej kategori, ponieważ pisaiada przypisane teksty";
            }

            return View(category);
        }

        // POST: Categories/Delete/5
        [Authorize(Roles = "admin,dyrektor")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Categories == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Categories'  is null.");
            }
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                if (TextsInCategory(id))
                {
                    _logger.LogWarning("Odmowa usunięcia kategorii {CategoryId}: ma przypisane teksty (użytkownik {UserId})",
                        id, User.FindFirstValue(ClaimTypes.NameIdentifier));
                    return RedirectToAction(nameof(Delete), new { id });
                }
                _context.Categories.Remove(category);
                _logger.LogInformation("Usunięto kategorię {CategoryId} (użytkownik {UserId})", id, User.FindFirstValue(ClaimTypes.NameIdentifier));
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
          return (_context.Categories?.Any(e => e.CategoryId == id)).GetValueOrDefault();
        }
        private bool CategoryNameExists(string? name)
        {
            return (_context.Categories?.Any(e => e.CategoryName == name)).GetValueOrDefault();
        }
        private bool TextsInCategory(int id)
        {
            // bez filtrów widoczności
            return (_context.Texts?.IgnoreQueryFilters().Any(t => t.CategoryId == id)).GetValueOrDefault();
        }
    }
}
