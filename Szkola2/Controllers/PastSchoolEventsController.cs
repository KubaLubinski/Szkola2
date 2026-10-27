using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Szkola2.Data;
using Szkola2.Infrastructure;
using Szkola2.Models;

namespace Szkola2.Controllers
{
    public class PastSchoolEventsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly IDemoContext _demo;
        private readonly FileCleaner _files;
        private readonly GalleryService _gallery;
        private readonly ILogger<PastSchoolEventsController> _logger;

        public PastSchoolEventsController(ApplicationDbContext context, IWebHostEnvironment environment,
            IDemoContext demo, FileCleaner files, GalleryService gallery, ILogger<PastSchoolEventsController> logger)
        {
            _context = context;
            _hostEnvironment = environment;
            _demo = demo;
            _files = files;
            _gallery = gallery;
            _logger = logger;
        }

        // GET: PastSchoolEvents
        public async Task<IActionResult> Index()
        {
            return _context.PastSchoolEvents != null ?
                        View(await _context.PastSchoolEvents.ToListAsync()) :
                        Problem("Entity set 'ApplicationDbContext.PastSchoolEvents'  is null.");
        }

        // GET: PastSchoolEvents/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.PastSchoolEvents == null)
            {
                return NotFound();
            }

            var pastSchoolEvent = await _context.PastSchoolEvents
                .Include(x => x.GraphicGallery)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (pastSchoolEvent == null)
            {
                return NotFound();
            }

            return View(pastSchoolEvent);
        }

        // GET: PastSchoolEvents/Create
        [Authorize(Roles = "admin,dyrektor")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: PastSchoolEvents/Create
        [Authorize(Roles = "admin,dyrektor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EventName,EventDate,Content")] PastSchoolEvent pastSchoolEvent, IFormFile? picture, IFormFileCollection? GraphicGalleryFiles)
        {
            pastSchoolEvent.Content = HtmlCleaner.Clean(pastSchoolEvent.Content);
            if (!ModelState.IsValid)
            {
                return View(pastSchoolEvent);
            }

            if (_demo.IsDemo)
            {
                if (await _context.PastSchoolEvents.CountAsync(e => e.DemoOwnerId == _demo.UserId) >= DemoLimits.MaxEvents)
                    return CreateFail(pastSchoolEvent, $"Konto testowe może mieć maksymalnie {DemoLimits.MaxEvents} własnych wpisów kalendarium. Usuń któryś, aby dodać nowy.");

                var limitError = DemoLimits.CheckFiles(picture, GraphicGalleryFiles);
                if (limitError != null) return CreateFail(pastSchoolEvent, limitError);
            }

            if (EventNameExist(pastSchoolEvent.EventName))
            {
                return CreateFail(pastSchoolEvent, "Wpis o takiej nazwie już istnieje!");
            }

            string? mainName = null;
            GalleryUploadResult? gallery = null;
            try
            {
                if (picture != null && picture.Length > 0)
                {
                    var sent = new ImageFileUpload(_hostEnvironment).SendFile(picture, "img", 600);
                    if (!sent.Success)
                    {
                        return CreateFail(pastSchoolEvent, "Wybrany plik nie jest obrazkiem!");
                    }
                    mainName = sent.Name;
                    pastSchoolEvent.MainGraphic = mainName;
                }

                gallery = _gallery.Save(GraphicGalleryFiles);
                if (!gallery.Success)
                {
                    _files.DeleteMainGraphic(mainName);
                    return CreateFail(pastSchoolEvent, gallery.Error!);
                }
                pastSchoolEvent.GraphicGallery = gallery.Items;

                _context.Add(pastSchoolEvent);
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

        // GET: PastSchoolEvents/Edit/5
        [Authorize(Roles = "admin,dyrektor")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.PastSchoolEvents == null)
            {
                return NotFound();
            }

            var pastSchoolEvent = await _context.PastSchoolEvents
                .Include(e => e.GraphicGallery)
                .FirstOrDefaultAsync(e => e.Id == id);
            if (pastSchoolEvent == null)
            {
                return NotFound();
            }

            if (_demo.IsDemo && !_demo.CanModify(pastSchoolEvent))
            {
                TempData[DemoMessages.Key] = DemoMessages.ViewOnly;
            }
            pastSchoolEvent.GraphicGallery ??= new List<Graphic>();
            return View(pastSchoolEvent);
        }

        // POST: PastSchoolEvents/Edit/5
        [Authorize(Roles = "admin,dyrektor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,EventName,EventDate,Content")] PastSchoolEvent pastSchoolEvent, IFormFile? picture, IFormFileCollection? GraphicGalleryFiles)
        {
            pastSchoolEvent.Content = HtmlCleaner.Clean(pastSchoolEvent.Content);
            if (id != pastSchoolEvent.Id)
            {
                return NotFound();
            }

            var entity = await _context.PastSchoolEvents
                .Include(e => e.GraphicGallery)
                .FirstOrDefaultAsync(e => e.Id == id);
            if (entity == null)
            {
                return NotFound();
            }

            if (!_demo.CanModify(entity))
            {
                TempData[DemoMessages.Key] = DemoMessages.ReadOnly;
                return RedirectToAction(nameof(Edit), new { id });
            }

            if (!ModelState.IsValid)
            {
                return EditFail(entity, pastSchoolEvent, null);
            }

            if (_demo.IsDemo)
            {
                var limitError = DemoLimits.CheckFiles(picture, GraphicGalleryFiles);
                if (limitError != null) return EditFail(entity, pastSchoolEvent, limitError);
            }

            if (EventNameExist(pastSchoolEvent.EventName, pastSchoolEvent.Id))
            {
                return EditFail(entity, pastSchoolEvent, "Wpis o takiej nazwie już istnieje!");
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
                        return EditFail(entity, pastSchoolEvent, "Wybrany plik nie jest obrazkiem!");
                    }
                    newMain = sent.Name;
                }

                gallery = _gallery.Save(GraphicGalleryFiles);
                if (!gallery.Success)
                {
                    _files.DeleteMainGraphic(newMain);
                    return EditFail(entity, pastSchoolEvent, gallery.Error);
                }

                entity.EventName = pastSchoolEvent.EventName;
                entity.EventDate = pastSchoolEvent.EventDate;
                entity.Content = pastSchoolEvent.Content;
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

            return RedirectToAction(nameof(Index));
        }

        // GET: PastSchoolEvents/Delete/5
        [Authorize(Roles = "admin,dyrektor")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.PastSchoolEvents == null)
            {
                return NotFound();
            }

            var pastSchoolEvent = await _context.PastSchoolEvents
                .FirstOrDefaultAsync(m => m.Id == id);
            if (pastSchoolEvent == null)
            {
                return NotFound();
            }

            return View(pastSchoolEvent);
        }

        // POST: PastSchoolEvents/Delete/5
        [Authorize(Roles = "admin,dyrektor")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.PastSchoolEvents == null)
            {
                return Problem("Entity set 'ApplicationDbContext.PastSchoolEvents'  is null.");
            }

            var pastSchoolEvent = await _context.PastSchoolEvents
                .Include(e => e.GraphicGallery)
                .FirstOrDefaultAsync(e => e.Id == id);
            if (pastSchoolEvent == null)
            {
                return RedirectToAction(nameof(Index));
            }

            if (!_demo.CanModify(pastSchoolEvent))
            {
                TempData[DemoMessages.Key] = DemoMessages.ReadOnly;
                return RedirectToAction(nameof(Index));
            }

            var mainGraphic = pastSchoolEvent.MainGraphic;
            var galleryUrls = (pastSchoolEvent.GraphicGallery ?? new List<Graphic>()).Select(g => g.URL).ToList();

            if (pastSchoolEvent.GraphicGallery != null) _context.GraphicGallery.RemoveRange(pastSchoolEvent.GraphicGallery);
            _context.PastSchoolEvents.Remove(pastSchoolEvent);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Usunięto wydarzenie {EventId} (użytkownik {UserId}, zdjęć w galerii: {GalleryCount})",
                id, User.FindFirstValue(ClaimTypes.NameIdentifier), galleryUrls.Count);

            _files.DeleteMainGraphic(mainGraphic);
            foreach (var url in galleryUrls) _files.DeleteGalleryImage(url);

            return RedirectToAction(nameof(Index));
        }

        public IActionResult YearEvents(int? year)
        {
            if (!year.HasValue)
            {
                return View("Error");
            }

            ViewBag.SelectedYear = year;
            ViewBag.AfterSelectedYear = year + 1;

            var events = _context.PastSchoolEvents
                .Where(e => e.EventDate >= new DateTime(year.Value, 9, 1) && e.EventDate < new DateTime(year.Value + 1, 9, 1))
                .Include(e => e.GraphicGallery)
                .ToList();

            return View(events);
        }

        [HttpDelete]
        [Authorize(Roles = "admin,dyrektor")]
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

        private IActionResult CreateFail(PastSchoolEvent model, string message)
        {
            ViewBag.ErrorMessage = message;
            return View("Create", model);
        }

        private IActionResult EditFail(PastSchoolEvent entity, PastSchoolEvent posted, string? message)
        {
            if (message != null) ViewBag.ErrorMessage = message;
            posted.MainGraphic = entity.MainGraphic;
            posted.GraphicGallery = entity.GraphicGallery ?? new List<Graphic>();
            return View("Edit", posted);
        }

        private bool EventNameExist(string? eventName)
        {
            return (_context.PastSchoolEvents?.Any(e => e.EventName == eventName)).GetValueOrDefault();
        }
        private bool EventNameExist(string? eventName, int? id)
        {
            return (_context.PastSchoolEvents?.Any(e => e.EventName == eventName & e.Id != id)).GetValueOrDefault();
        }
    }
}
