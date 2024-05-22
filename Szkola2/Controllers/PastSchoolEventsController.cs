using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Humanizer;
using LazZiya.ImageResize;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Szkola2.Data;
using Szkola2.Infrastructure;
using Szkola2.Models;
using static System.Net.Mime.MediaTypeNames;
using Image = System.Drawing.Image;

namespace Szkola2.Controllers
{
    public class PastSchoolEventsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private IWebHostEnvironment _hostEnvironment;

        public PastSchoolEventsController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _hostEnvironment = environment;
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
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "admin,dyrektor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,EventName,EventDate,Content")] PastSchoolEvent pastSchoolEvent, IFormFile? picture, IFormFileCollection? GraphicGalleryFiles)

        {
            if (ModelState.IsValid)
            {
                if (picture != null && picture.Length > 0)
                {
                    ImageFileUpload imageFileResult = new(_hostEnvironment);
                    FileSendResult fileSendResult = imageFileResult.SendFile(picture, "img", 600);

                    if (fileSendResult.Success)
                    {
                        pastSchoolEvent.MainGraphic = fileSendResult.Name;
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Wybrany plik nie jest obrazkiem!";
                        return View(pastSchoolEvent);
                    }
                }

                if (GraphicGalleryFiles != null)
                {
                    ImageFileUpload imageFileUpload = new(_hostEnvironment);
                    string folder = "gallery\\";
                    pastSchoolEvent.GraphicGallery = new List<Graphic>();

                    foreach (var file in GraphicGalleryFiles)
                    {
                        string extension = Path.GetExtension(file.FileName);
                        if (imageFileUpload.FileTypeCheck(extension))
                        {
                            var gallery = new Graphic()
                            {
                                Name = file.FileName,
                                URL = imageFileUpload.UploadImage(folder, file)
                            };
                            pastSchoolEvent.GraphicGallery.Add(gallery);
                            string path = Path.Combine(_hostEnvironment.WebRootPath + gallery.URL);
                            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                            {
                                using (Image imgFile = Image.FromStream(fs))
                                {
                                    var miniFile = imgFile.ScaleByHeight(600);
                                    var upload = Path.Combine(_hostEnvironment.WebRootPath + "\\miniGallery\\" + gallery.URL);
                                    miniFile.SaveAs(upload);
                                }
                            };
                        }
                        else
                        {
                            ViewBag.ErrorMessage = "Jeden z wybranych plików nie jest obrazkiem!";
                            return View(pastSchoolEvent);
                        }
                    }
                }
                if (!EventNameExist(pastSchoolEvent.EventName))
                {
                    _context.Add(pastSchoolEvent);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ViewBag.ErrorMessage = "Wpis o takiej nazwie już istnieje!";
                    return View(pastSchoolEvent);
                }
            }
            return View(pastSchoolEvent);
        }

        // GET: PastSchoolEvents/Edit/5
        [Authorize(Roles = "admin,dyrektor")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.PastSchoolEvents == null)
            {
                return NotFound();
            }

            var pastSchoolEvent = await _context.PastSchoolEvents.FindAsync(id);
            if (pastSchoolEvent == null)
            {
                return NotFound();
            }
            pastSchoolEvent.GraphicGallery = _context.GraphicGallery?.Where(g => g.EventId == id).ToList();
            return View(pastSchoolEvent);
        }

        // POST: PastSchoolEvents/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "admin,dyrektor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,EventName,MainGraphic,EventDate,Content")] PastSchoolEvent pastSchoolEvent, IFormFile? picture, IFormFileCollection? GraphicGalleryFiles)
        {
            if (id != pastSchoolEvent.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (EventNameExist(pastSchoolEvent.EventName, pastSchoolEvent.Id))
                {
                    ViewBag.ErrorMessage = "Wpis o takiej nazwie już istnieje!";
                    pastSchoolEvent.GraphicGallery = _context.GraphicGallery?.Where(g => g.EventId == id).ToList();;
                    return View(pastSchoolEvent);
                }
                if (picture != null && picture.Length > 0)
                {
                    ImageFileUpload imageFileResult = new(_hostEnvironment);
                    FileSendResult fileSendResult = imageFileResult.SendFile(picture, "img", 600);

                    if (fileSendResult.Success)
                    {
                        pastSchoolEvent.MainGraphic = fileSendResult.Name;
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Wybrany plik nie jest obrazkiem!";
                        pastSchoolEvent.GraphicGallery = _context.GraphicGallery?.Where(g => g.EventId == id).ToList();
                        return View(pastSchoolEvent);
                    }
                }
                //test graphic
                if (GraphicGalleryFiles != null)
                {
                    ImageFileUpload imageFileUpload = new(_hostEnvironment);
                    string folder = "gallery\\";

                    pastSchoolEvent.GraphicGallery = new List<Graphic>();

                    foreach (var file in GraphicGalleryFiles)
                    {
                        string extension = Path.GetExtension(file.FileName);

                        if (imageFileUpload.FileTypeCheck(extension))
                        {
                            var gallery = new Graphic()
                            {
                                Name = file.FileName,
                                URL = imageFileUpload.UploadImage(folder, file)
                            };

                            pastSchoolEvent.GraphicGallery.Add(gallery);

                            string path = Path.Combine(_hostEnvironment.WebRootPath + gallery.URL);

                            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                            {
                                using (Image imgFile = Image.FromStream(fs))
                                {
                                    var miniFile = imgFile.ScaleByHeight(600);
                                    var upload = Path.Combine(_hostEnvironment.WebRootPath + "\\miniGallery\\" + gallery.URL);
                                    miniFile.SaveAs(upload);
                                }
                            }
                        }
                        else
                        {
                            ViewBag.ErrorMessage = "Jeden z wybranych plików nie jest obrazkiem!";
                            pastSchoolEvent.GraphicGallery = _context.GraphicGallery?.Where(g => g.EventId == id).ToList();
                            return View(pastSchoolEvent);
                        }
                    }
                }
                try
                {
                    _context.Update(pastSchoolEvent);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PastSchoolEventExists(pastSchoolEvent.Id))
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
            pastSchoolEvent.GraphicGallery = _context.GraphicGallery?.Where(g => g.EventId == id).ToList();
            return View(pastSchoolEvent);
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
            var pastSchoolEvent = await _context.PastSchoolEvents.FindAsync(id);
            if (pastSchoolEvent != null)
            {
                var MainGraphicDelete = Path.Combine(_hostEnvironment.WebRootPath + pastSchoolEvent.MainGraphic);
                DeleteImage(MainGraphicDelete);

                var ImagesToDelete = _context.GraphicGallery.Where(u => u.Id == id).ToList();
                foreach (var image in ImagesToDelete)
                {
                    var DeleteImagePath = Path.Combine(_hostEnvironment.WebRootPath + image.URL);
                    var DeleteMiniImagePath = Path.Combine(_hostEnvironment.WebRootPath + "\\miniGallery" + image.URL);
                    DeleteImage(DeleteImagePath);
                    DeleteImage(DeleteMiniImagePath);
                }
                var HistoryGalleriesItemToDelete = _context.GraphicGallery.Where(i => i.EventId == id).ToList();
                foreach (var item in HistoryGalleriesItemToDelete)
                {
                    _context.GraphicGallery.Remove(item);
                }
                _context.PastSchoolEvents.Remove(pastSchoolEvent);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        //public async Task<IActionResult> YearEvents(int? year)
        //{
        //    var applicationDbContext = _context.PastSchoolEvents.Where(e => e.EventDate.Year == year.Value);
        //    return View(await applicationDbContext.ToListAsync());
        //}

        public IActionResult YearEvents(int? year)
        {
            if (!year.HasValue)
            {
                return View("Error");
            }

            if (year.HasValue)
            {
                ViewBag.SelectedYear = year;
                ViewBag.AfterSelectedYear = year + 1;
            }

            List<string> numberWords = new List<string>();
            for (int i = 2; i <= 100; i++)
            {
                string word = i.ToWords().Transform(To.TitleCase);
                numberWords.Add(word);
            }

            var events = _context.PastSchoolEvents
                .Where(e => e.EventDate >= new DateTime(year.Value, 9, 1) && e.EventDate < new DateTime(year.Value + 1, 9, 1))
                .Include(e => e.GraphicGallery)
                .ToList();

            return View(events);
        }
        private bool PastSchoolEventExists(int id)
        {
          return (_context.PastSchoolEvents?.Any(e => e.Id == id)).GetValueOrDefault();
        }
        private bool EventNameExist(string? eventName)
        {
            return (_context.PastSchoolEvents?.Any(e => e.EventName == eventName)).GetValueOrDefault();
        }
        private bool EventNameExist(string? eventName, int? id)
        {
            return (_context.PastSchoolEvents?.Any(e => e.EventName == eventName & e.Id != id)).GetValueOrDefault();
        }
        public IActionResult DeleteOneImage(int Id)
        {
            var ImageToDelete = _context.GraphicGallery.FirstOrDefault(u => u.Id == Id);
            var DeleteImagePath = Path.Combine(_hostEnvironment.WebRootPath + ImageToDelete.URL);
            var DeleteMiniImagePath = Path.Combine(_hostEnvironment.WebRootPath + "\\miniGallery" + ImageToDelete.URL);
            DeleteImage(DeleteImagePath);
            DeleteImage(DeleteMiniImagePath);

            _context.GraphicGallery.Remove(ImageToDelete);
            _context.SaveChanges();
            return RedirectToAction(nameof(Edit));
        }
        private void DeleteImage(string deleteImagePath)
        {
            if (System.IO.File.Exists(deleteImagePath))
            {
                System.IO.File.Delete(deleteImagePath);
            }
        }
    }
}
