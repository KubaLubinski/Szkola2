using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Szkola2.Data;
using Szkola2.Infrastructure;
using Szkola2.Models;
using Szkola2.Models.ViewModels;

namespace Szkola2.Controllers
{
    public class TextsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private IWebHostEnvironment _hostEnvironment;

        public TextsController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _hostEnvironment = environment;
        }

        // GET: Texts
        public async Task<IActionResult> Index(string Fraza, string Autor, int? Kategoria, int PageNumber = 1)
        {
            var SelectedTexts = _context.Texts?
                .Include(t => t.Category)
                .Include(t => t.User)
                .Where(t => t.IsActive == true)
                .OrderByDescending(t => t.DateAdded);

            if (Kategoria != null)
            {
                SelectedTexts = (IOrderedQueryable<Text>)SelectedTexts.Where(r => r.Category.CategoryId == Kategoria);
            }
            if (!String.IsNullOrEmpty(Autor))
            {
                SelectedTexts = (IOrderedQueryable<Text>)SelectedTexts.Where(r => r.User.Id == Autor);
            }
            if (!String.IsNullOrEmpty(Fraza))
            {
                SelectedTexts = (IOrderedQueryable<Text>)SelectedTexts.Where(r => r.Content.Contains(Fraza));
            }

            TextsViewModel textsViewModel = new();
            
            textsViewModel.TextsView = new TextsView();

            

            textsViewModel.TextsView.TextCount = SelectedTexts.Count();

            textsViewModel.TextsView.PageNumber = PageNumber;

            textsViewModel.TextsView.Author = Autor;
            textsViewModel.TextsView.Pharse = Fraza;
            textsViewModel.TextsView.Category = Kategoria;

            textsViewModel.Texts = (IEnumerable<Text>?) await SelectedTexts
                .Skip((PageNumber -1) * textsViewModel.TextsView.PageSize)
                .Take(textsViewModel.TextsView.PageSize)
                .ToListAsync();

            

            ViewData["Category"] = new SelectList(_context.Categories?
                .Where(c => c.IsActive == true),
                "CategoryId", "CategoryName", Kategoria);
            ViewData["Author"] = new SelectList(_context.Texts
                .Include(u => u.User)
                .Select(u => u.User)
                .Distinct(),
                "Id","LastName", Autor);

            return View(textsViewModel);
        }
        // GET: Texts
        [Authorize(Roles ="admin,dyrektor")]
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
        public async Task<IActionResult> Create([Bind("TextId,TextName,Content,DateAdded,CategoryId,IsActive,Id")] Text text, IFormFile? picture, IFormFileCollection? GalleryFiles)
        {
            if (ModelState.IsValid)
            {
                text.Id = User.FindFirstValue(ClaimTypes.NameIdentifier);
                text.DateAdded = DateTime.Now;

                //zapisanie obrazka i skalowanie
                if (picture != null && picture.Length > 0)
                {
                    ImageFileUpload imageFileResult = new(_hostEnvironment);
                    FileSendResult fileSendResult = imageFileResult.SendFile(picture, "img", 600);

                    if (fileSendResult.Success)
                    {
                        text.MainGraphic = fileSendResult.Name;
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Wybrany plik nie jest obrazkiem!";
                        ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName", text.CategoryId);
                        return View(text);
                    }
                    //_context.Add(text);
                    //await _context.SaveChangesAsync();
                    //return RedirectToAction(nameof(Index));
                }

                //dodawanie wielu grafik
                if (GalleryFiles != null)
                {
                    string folder = "gallery/";

                    text.Gallerys = new List<Gallery>();
                    
                    foreach (var file in GalleryFiles)
                    {
                        var gallery = new Gallery()
                        {
                            Name = file.Name,
                            URL = await UploadImage(folder, file)
                        };

                        text.Gallerys.Add(gallery);
                    }
                }

                if (!TextNameExist(text.TextName))
                {
                    _context.Add(text);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ViewBag.ErrorMessage = "Wpis o takiej nazwie już istnieje!";
                    ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName", text.CategoryId);
                    return View(text);
                }
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
            if (string.Compare(User.FindFirstValue(ClaimTypes.NameIdentifier), text.Id) == 0 || User.IsInRole("admin") || User.IsInRole("dyrektor"))
            {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName", text.CategoryId);
             ViewData["Author"] = text.Id;
            return View(text);
            }
            else
            {
                return RedirectToAction(nameof(Index));
            }
           
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

        private bool TextNameExist(string? name)
        {
            return (_context.Texts?.Any(e => e.TextName == name)).GetValueOrDefault();
        }

        private async Task<string> UploadImage(string folderPath, IFormFile file)
        {
            folderPath += Guid.NewGuid().ToString() + "_" + file.FileName;
            string serverFolder = Path.Combine(_hostEnvironment.WebRootPath, folderPath);
            await file.CopyToAsync(new FileStream(serverFolder, FileMode.Create));
            return "/" + folderPath;
        }

    }
}
