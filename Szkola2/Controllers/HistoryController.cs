using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Szkola2.Data;
using Szkola2.Infrastructure;
using Szkola2.Models;

namespace Szkola2.Controllers
{
    public class HistoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private IWebHostEnvironment _hostEnvironment;
        private readonly IDemoContext _demo;
        private readonly FileCleaner _files;

        public HistoryController(ApplicationDbContext context, IWebHostEnvironment environment,
            IDemoContext demo, FileCleaner files)
        {
            _context = context;
            _hostEnvironment = environment;
            _demo = demo;
            _files = files;
        }

        // GET: History
        public async Task<IActionResult> Index()
        {
            return _context.History != null ?
                        View(await _context.History.ToListAsync()) :
                        Problem("Entity set 'ApplicationDbContext.History'  is null.");
        }

        // GET: History/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.History == null)
            {
                return NotFound();
            }

            var history = await _context.History
                .Include(g => g.GraphicGallery)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (history == null)
            {
                return NotFound();
            }

            return View(history);
        }

        // GET: History/Create
        [Authorize(Roles = "admin,dyrektor")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: History/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "admin,dyrektor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TextName,Content")] History history, IFormFileCollection GraphicGalleryFiles)
        {
            history.Content = HtmlCleaner.Clean(history.Content);
            if (_demo.IsDemo)
            {
                TempData[DemoMessages.Key] = DemoMessages.ReadOnly;
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                if (GraphicGalleryFiles != null)
                {
                    ImageFileUpload imageFileUpload = new(_hostEnvironment);
                    history.GraphicGallery = new List<Graphic>();

                    foreach (var file in GraphicGalleryFiles)
                    {
                        if (imageFileUpload.TryUploadGalleryImage(file, out var galleryUrl, out _))
                        {
                            history.GraphicGallery.Add(new Graphic() { Name = Path.GetFileName(file.FileName), URL = galleryUrl });
                        }
                        else
                        {
                            // usuwamy zdjęcia zapisane wcześniej w tej samej partii
                            foreach (var uploaded in history.GraphicGallery) _files.DeleteGalleryImage(uploaded.URL);
                            ViewBag.ErrorMessage = "Jeden z wybranych plików nie jest obrazkiem!";
                            return View(history);
                        }
                    } 
                }
                if (!TextNameExist(history.TextName))
                {
                    _context.Add(history);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ViewBag.ErrorMessage = "Wpis o takiej nazwie już istnieje!";
                    return View(history);
                }
            }
            return View(history);
        }

        // GET: History/Edit/5
        [Authorize(Roles = "admin,dyrektor")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.History == null)
            {
                return NotFound();
            }

            var history = await _context.History.FindAsync(id);
            if (history == null)
            {
                return NotFound();
            }
            history.GraphicGallery = _context.GraphicGallery?.Where(g => g.HistoryId == id).ToList();
            return View(history);
        }

        // POST: History/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "admin,dyrektor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TextName,Content")] History history, IFormFileCollection GraphicGalleryFiles)
        {
            history.Content = HtmlCleaner.Clean(history.Content);
            if (_demo.IsDemo)
            {
                TempData[DemoMessages.Key] = DemoMessages.ReadOnly;
                return RedirectToAction(nameof(Edit), new { id });
            }

            if (id != history.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (TextNameExist(history.TextName, history.Id))
                {
                    ViewBag.ErrorMessage = "Wpis o takiej nazwie już istnieje!";
                    history.GraphicGallery = _context.GraphicGallery?.Where(g => g.EventId == id).ToList();
                    return View(history);
                }
                if (GraphicGalleryFiles != null)
                {
                    ImageFileUpload imageFileUpload = new(_hostEnvironment);

                    history.GraphicGallery = new List<Graphic>();

                    foreach (var file in GraphicGalleryFiles)
                    {
                        if (imageFileUpload.TryUploadGalleryImage(file, out var galleryUrl, out _))
                        {
                            history.GraphicGallery.Add(new Graphic() { Name = Path.GetFileName(file.FileName), URL = galleryUrl });
                        }
                        else
                        {
                            // usuwamy zdjęcia zapisane wcześniej w tej samej partii
                            foreach (var uploaded in history.GraphicGallery) _files.DeleteGalleryImage(uploaded.URL);
                            ViewBag.ErrorMessage = "Jeden z wybranych plików nie jest obrazkiem!";
                            history.GraphicGallery = _context.GraphicGallery?.Where(g => g.EventId == id).ToList();
                            return View(history);
                        }
                    }
                }
                try
                {
                    _context.Update(history);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HistoryExists(history.Id))
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
            history.GraphicGallery = _context.GraphicGallery?.Where(g => g.EventId == id).ToList();
            return View(history);
        }

        // GET: History/Delete/5
        [Authorize(Roles = "admin,dyrektor")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.History == null)
            {
                return NotFound();
            }

            var history = await _context.History
                .FirstOrDefaultAsync(m => m.Id == id);
            if (history == null)
            {
                return NotFound();
            }

            return View(history);
        }

        // POST: History/Delete/5
        [Authorize(Roles = "admin,dyrektor")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirm(int id)
        {
            if (_context.History == null)
            {
                return Problem("Entity set 'ApplicationDbContext.History'  is null.");
            }

            // konto testowe nie może niczego usuwać (wcześniej pliki były kasowane z dysku, zanim zapis do bazy został odrzucony)
            if (_demo.IsDemo)
            {
                TempData[DemoMessages.Key] = DemoMessages.ReadOnly;
                return RedirectToAction(nameof(Index));
            }

            var history = await _context.History.FirstOrDefaultAsync(h => h.Id == id);
            if (history == null)
            {
                return RedirectToAction(nameof(Index));
            }

            // wcześniej: Where(u => u.Id == id) - kasowało pliki cudzych zdjęć o tym samym numerze
            var images = await _context.GraphicGallery!.Where(g => g.HistoryId == id).ToListAsync();
            var urls = images.Select(g => g.URL).ToList();

            _context.GraphicGallery.RemoveRange(images);
            _context.History.Remove(history);
            await _context.SaveChangesAsync();

            // pliki dopiero po udanym zapisie
            foreach (var url in urls) _files.DeleteGalleryImage(url);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult List()
        {
            var HistoryTexts = _context.History.Include(t => t.GraphicGallery).ToList();
            return View(HistoryTexts);

        }

        // Wywoływane przez DeleteImage.js: $.ajax({ type: "DELETE" }) i oczekuje JSON { success, message }.
        // Wcześniej: brak autoryzacji, dowolna metoda HTTP (także GET) i wyjątek, gdy grafika nie istnieje.
        [HttpDelete]
        [Authorize(Roles = "admin,dyrektor")]
        public async Task<IActionResult> DeleteOneImage(int Id)
        {
            if (_demo.IsDemo)
            {
                return Json(new { success = false, message = DemoMessages.ReadOnly });
            }

            var image = await _context.GraphicGallery!.FirstOrDefaultAsync(g => g.Id == Id && g.HistoryId != null);
            if (image == null)
            {
                return Json(new { success = false, message = "Nie znaleziono grafiki." });
            }

            var url = image.URL;
            _context.GraphicGallery.Remove(image);
            await _context.SaveChangesAsync();
            _files.DeleteGalleryImage(url);

            return Json(new { success = true, message = "Grafika została usunięta." });
        }

        private void DeleteImage(string deleteImagePath)
        {
            if (System.IO.File.Exists(deleteImagePath))
            {
                System.IO.File.Delete(deleteImagePath);
            }
        }

        private bool HistoryExists(int id)
        {
            return (_context.History?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        private bool TextNameExist(string? textName)
        {
            return (_context.History?.Any(e => e.TextName == textName)).GetValueOrDefault();
        }
        private bool TextNameExist(string? textName, int? id)
        {
            return (_context.History?.Any(e => e.TextName == textName & e.Id != id)).GetValueOrDefault();
        }
    }
}
