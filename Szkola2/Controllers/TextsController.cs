using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Szkola2.Data;
using Szkola2.Infrastructure;
using Szkola2.Models;
using Szkola2.Models.ViewModels;

namespace Szkola2.Controllers
{
    public class TextsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly IDemoContext _demo;
        private readonly FileCleaner _files;
        private readonly GalleryService _gallery;
        private readonly ILogger<TextsController> _logger;

        public TextsController(ApplicationDbContext context, IWebHostEnvironment environment,
            IDemoContext demo, FileCleaner files, GalleryService gallery, ILogger<TextsController> logger)
        {
            _context = context;
            _hostEnvironment = environment;
            _demo = demo;
            _files = files;
            _gallery = gallery;
            _logger = logger;
        }

        // GET: Texts
        public async Task<IActionResult> Index(string Fraza, string Autor, int? Kategoria, int PageNumber = 1)
        {
            // numer strony z adresu URL: wartość <= 0 dawała ujemny Skip i błąd 500
            if (PageNumber < 1) PageNumber = 1;

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

            textsViewModel.Texts = (IEnumerable<Text>?)await SelectedTexts
                .Skip((PageNumber - 1) * textsViewModel.TextsView.PageSize)
                .Take(textsViewModel.TextsView.PageSize)
                .ToListAsync();

            ViewData["Category"] = new SelectList(_context.Categories?
                .Where(c => c.IsActive == true),
                "CategoryId", "CategoryName", Kategoria);
            ViewData["Author"] = new SelectList(_context.Texts
                .Include(u => u.User)
                .Select(u => u.User)
                .Distinct(),
                "Id", "LastName", Autor);

            return View(textsViewModel);
        }

        // GET: Texts/List
        [Authorize(Roles = "admin,dyrektor")]
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
                .Include(g => g.GraphicGallery)
                .FirstOrDefaultAsync(m => m.TextId == id);
            if (text == null || !CanSeeText(text))
            {
                return NotFound();
            }

            return View(text);
        }

        // GET: Texts/Create
        [Authorize(Roles = "admin,dyrektor,nauczyciel")]
        public IActionResult Create()
        {
            FillCategories();
            return View();
        }

        // POST: Texts/Create
        // DemoOwnerId, Id (autor) i DateAdded ustawiamy po stronie serwera - nie są bindowane z formularza.
        [Authorize(Roles = "admin,dyrektor,nauczyciel")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TextName,Content,CategoryId,EventDate,IsActive")] Text text, IFormFile? picture, IFormFileCollection? GraphicGalleryFiles)
        {
            text.Content = HtmlCleaner.Clean(text.Content);
            if (!(text.CategoryId == 1 || text.CategoryId == 2))
            {
                ModelState.Remove("EventDate");
            }

            if (!ModelState.IsValid)
            {
                FillCategories(text.CategoryId);
                return View(text);
            }

            if (_demo.IsDemo)
            {
                if (await _context.Texts.CountAsync(t => t.DemoOwnerId == _demo.UserId) >= DemoLimits.MaxTexts)
                    return CreateFail(text, $"Konto testowe może mieć maksymalnie {DemoLimits.MaxTexts} własnych tekstów. Usuń któryś, aby dodać nowy.");

                var limitError = DemoLimits.CheckFiles(picture, GraphicGalleryFiles);
                if (limitError != null) return CreateFail(text, limitError);
            }

            if (TextNameExist(text.TextName))
            {
                return CreateFail(text, "Wpis o takiej nazwie już istnieje!");
            }

            text.Id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            text.DateAdded = DateTime.Now;

            string? mainName = null;
            GalleryUploadResult? gallery = null;
            try
            {
                if (picture != null && picture.Length > 0)
                {
                    var sent = new ImageFileUpload(_hostEnvironment).SendFile(picture, "img", 600);
                    if (!sent.Success)
                    {
                        return CreateFail(text, "Wybrany plik nie jest obrazkiem!");
                    }
                    mainName = sent.Name;
                    text.MainGraphic = mainName;
                }

                gallery = _gallery.Save(GraphicGalleryFiles);
                if (!gallery.Success)
                {
                    _files.DeleteMainGraphic(mainName);
                    return CreateFail(text, gallery.Error!);
                }
                text.GraphicGallery = gallery.Items;

                _context.Add(text);
                await _context.SaveChangesAsync();
            }
            catch
            {
                _files.DeleteMainGraphic(mainName);
                if (gallery != null) _gallery.Discard(gallery);
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Texts/Edit/5
        // Widok edycji jest dostępny także dla cudzych wpisów (podgląd); blokada następuje dopiero przy zapisie.
        [Authorize(Roles = "admin,dyrektor,nauczyciel")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Texts == null)
            {
                return NotFound();
            }
            var text = await _context.Texts
                .Include(t => t.GraphicGallery)
                .FirstOrDefaultAsync(t => t.TextId == id);
            if (text == null)
            {
                return NotFound();
            }
            if (string.Compare(User.FindFirstValue(ClaimTypes.NameIdentifier), text.Id) == 0 || User.IsInRole("admin") || User.IsInRole("dyrektor"))
            {
                if (_demo.IsDemo && !_demo.CanModify(text))
                {
                    TempData[DemoMessages.Key] = DemoMessages.ViewOnly;
                }
                FillCategories(text.CategoryId);
                ViewData["Author"] = text.Id;
                text.GraphicGallery ??= new List<Graphic>();
                return View(text);
            }
            else
            {
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Texts/Edit/5
        // Edytujemy rekord załadowany z bazy i przepisujemy tylko dozwolone pola
        // (autor, data dodania, zdjęcie główne i DemoOwnerId nie pochodzą z formularza).
        [Authorize(Roles = "admin,dyrektor,nauczyciel")]
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int textid, [Bind("TextId,TextName,Content,CategoryId,EventDate,IsActive")] Text text, IFormFile? picture, IFormFileCollection? GraphicGalleryFiles)
        {
            text.Content = HtmlCleaner.Clean(text.Content);
            if (textid != text.TextId)
            {
                return NotFound();
            }

            var entity = await _context.Texts
                .Include(t => t.GraphicGallery)
                .FirstOrDefaultAsync(t => t.TextId == textid);
            if (entity == null)
            {
                return NotFound();
            }

            if (!_demo.CanModify(entity))
            {
                TempData[DemoMessages.Key] = DemoMessages.ReadOnly;
                return RedirectToAction(nameof(Edit), new { id = textid });
            }

            if (!(User.FindFirstValue(ClaimTypes.NameIdentifier) == entity.Id || User.IsInRole("admin") || User.IsInRole("dyrektor")))
            {
                return RedirectToAction(nameof(Index));
            }

            if (!(text.CategoryId == 1 || text.CategoryId == 2))
            {
                ModelState.Remove("EventDate");
            }

            if (!ModelState.IsValid)
            {
                return EditFail(entity, text, null);
            }

            if (_demo.IsDemo)
            {
                var limitError = DemoLimits.CheckFiles(picture, GraphicGalleryFiles);
                if (limitError != null) return EditFail(entity, text, limitError);
            }

            if (TextNameExist(text.TextName, text.TextId))
            {
                return EditFail(entity, text, "Wpis o takiej nazwie już istnieje!");
            }

            string? newMain = null;
            var oldMain = entity.MainGraphic;
            GalleryUploadResult? gallery = null;
            try
            {
                if (picture != null && picture.Length > 0)
                {
                    var sent = new ImageFileUpload(_hostEnvironment).SendFile(picture, "img", 600);
                    if (!sent.Success)
                    {
                        return EditFail(entity, text, "Wybrany plik nie jest obrazkiem!");
                    }
                    newMain = sent.Name;
                }

                gallery = _gallery.Save(GraphicGalleryFiles);
                if (!gallery.Success)
                {
                    _files.DeleteMainGraphic(newMain);
                    return EditFail(entity, text, gallery.Error);
                }

                entity.TextName = text.TextName;
                entity.Content = text.Content;
                entity.CategoryId = text.CategoryId;
                entity.EventDate = text.EventDate;
                entity.IsActive = text.IsActive;
                if (newMain != null) entity.MainGraphic = newMain;

                entity.GraphicGallery ??= new List<Graphic>();
                foreach (var g in gallery.Items) entity.GraphicGallery.Add(g);

                await _context.SaveChangesAsync();
            }
            catch
            {
                _files.DeleteMainGraphic(newMain);
                if (gallery != null) _gallery.Discard(gallery);
                throw;
            }

            if (newMain != null) _files.DeleteMainGraphic(oldMain);

            return User.IsInRole("admin") || User.IsInRole("dyrektor")
                ? RedirectToAction(nameof(List))
                : RedirectToAction(nameof(Index));
        }

        // GET: Texts/Delete/5
        [Authorize(Roles = "admin,dyrektor")]
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
        [Authorize(Roles = "admin,dyrektor")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Texts == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Texts'  is null.");
            }

            var text = await _context.Texts
                .Include(t => t.GraphicGallery)
                .FirstOrDefaultAsync(t => t.TextId == id);
            if (text == null)
            {
                return RedirectToAction(nameof(List));
            }

            if (!_demo.CanModify(text))
            {
                TempData[DemoMessages.Key] = DemoMessages.ReadOnly;
                return RedirectToAction(nameof(List));
            }

            var mainGraphic = text.MainGraphic;
            var galleryUrls = (text.GraphicGallery ?? new List<Graphic>()).Select(g => g.URL).ToList();

            if (text.GraphicGallery != null) _context.GraphicGallery.RemoveRange(text.GraphicGallery);
            _context.Texts.Remove(text);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Usunięto tekst {TextId} (użytkownik {UserId}, zdjęć w galerii: {GalleryCount})",
                id, User.FindFirstValue(ClaimTypes.NameIdentifier), galleryUrls.Count);

            // pliki kasujemy dopiero po udanym zapisie w bazie
            _files.DeleteMainGraphic(mainGraphic);
            foreach (var url in galleryUrls) _files.DeleteGalleryImage(url);

            return RedirectToAction(nameof(List));
        }

        // Wywoływane przez DeleteImage.js: $.ajax({ type: "DELETE" }) i oczekuje JSON { success, message }.
        [HttpDelete]
        [Authorize(Roles = "admin,dyrektor,nauczyciel")]
        public async Task<IActionResult> DeleteOneImage(int Id)
        {
            var result = await _gallery.DeleteOneAsync(Id, User);
            if (!result.Found)
            {
                return Json(new { success = false, message = "Nie znaleziono grafiki." });
            }
            if (result.Forbidden)
            {
                return Json(new { success = false, message = "Brak uprawnień do tego zdjęcia." });
            }
            if (!result.Allowed)
            {
                return Json(new { success = false, message = DemoMessages.ReadOnly });
            }
            return Json(new { success = true, message = "Grafika została usunięta." });
        }

        /// <summary>Teksty ukryte ("Czy wyświetlać?" = nie) widzi tylko redakcja: autor, dyrektor i admin.</summary>
        private bool CanSeeText(Text text)
        {
            return text.IsActive
                   || User.IsInRole("admin")
                   || User.IsInRole("dyrektor")
                   || (User.Identity?.IsAuthenticated == true && text.Id == User.FindFirstValue(ClaimTypes.NameIdentifier));
        }

        private void FillCategories(int? selected = null)
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName", selected);
        }

        private IActionResult CreateFail(Text text, string message)
        {
            ViewBag.ErrorMessage = message;
            FillCategories(text.CategoryId);
            return View("Create", text);
        }

        private IActionResult EditFail(Text entity, Text posted, string? message)
        {
            if (message != null) ViewBag.ErrorMessage = message;
            FillCategories(posted.CategoryId);
            ViewData["Author"] = entity.Id;
            posted.Id = entity.Id;
            posted.DateAdded = entity.DateAdded;
            posted.MainGraphic = entity.MainGraphic;
            posted.GraphicGallery = entity.GraphicGallery ?? new List<Graphic>();
            return View("Edit", posted);
        }

        private bool TextNameExist(string? name, int? id)
        {
            return (_context.Texts?.Any(e => e.TextName == name & e.TextId != id)).GetValueOrDefault();
        }
        private bool TextNameExist(string? name)
        {
            return (_context.Texts?.Any(e => e.TextName == name)).GetValueOrDefault();
        }
    }
}
