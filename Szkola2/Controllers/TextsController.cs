using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Szkola2.Data;
using Szkola2.Models;

namespace Szkola2.Controllers
{
    public class TextsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TextsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Texts
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Texts.Include(t => t.Category).Include(t => t.User);
            return View(await applicationDbContext.ToListAsync());
        }
        // GET: Texts
        public async Task<IActionResult> List()
        {
            var applicationDbContext = _context.Texts.Include(t => t.Category).Include(t => t.User);
            return View(await applicationDbContext.ToListAsync());
        }
        // GET: Texts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Texts == null)
            {
                return NotFound();
            }

            var text = await _context.Texts
                .Include(t => t.Category)
                .Include(t => t.User)
                .FirstOrDefaultAsync(m => m.TextId == id);
            if (text == null)
            {
                return NotFound();
            }

            return View(text);
        }

        // GET: Texts/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName");
            ViewData["Id"] = new SelectList(_context.AppUsers, "Id", "Id");
            return View();
        }

        // POST: Texts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TextId,TextName,Content,DateAdded,MainGraphic,CategoryId,IsActive,Id")] Text text)
        {
            if (ModelState.IsValid)
            {
                _context.Add(text);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName", text.CategoryId);
            ViewData["Id"] = new SelectList(_context.AppUsers, "Id", "Id", text.Id);
            return View(text);
        }

        // GET: Texts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Texts == null)
            {
                return NotFound();
            }

            var text = await _context.Texts.FindAsync(id);
            if (text == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName", text.CategoryId);
            ViewData["Id"] = new SelectList(_context.AppUsers, "Id", "Id", text.Id);
            return View(text);
        }

        // POST: Texts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TextId,TextName,Content,DateAdded,MainGraphic,CategoryId,IsActive,Id")] Text text)
        {
            if (id != text.TextId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(text);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TextExists(text.TextId))
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
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName", text.CategoryId);
            ViewData["Id"] = new SelectList(_context.AppUsers, "Id", "Id", text.Id);
            return View(text);
        }

        // GET: Texts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Texts == null)
            {
                return NotFound();
            }

            var text = await _context.Texts
                .Include(t => t.Category)
                .Include(t => t.User)
                .FirstOrDefaultAsync(m => m.TextId == id);
            if (text == null)
            {
                return NotFound();
            }

            return View(text);
        }

        // POST: Texts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Texts == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Texts'  is null.");
            }
            var text = await _context.Texts.FindAsync(id);
            if (text != null)
            {
                _context.Texts.Remove(text);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TextExists(int id)
        {
          return (_context.Texts?.Any(e => e.TextId == id)).GetValueOrDefault();
        }
    }
}
