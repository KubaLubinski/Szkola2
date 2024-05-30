using Humanizer;
using LazZiya.ImageResize;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Szkola2.Data;
using Szkola2.Infrastructure;
using Szkola2.Models;
using Image = System.Drawing.Image;

namespace Szkola2.Controllers
{
    public class HistoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private IWebHostEnvironment _hostEnvironment;

        public HistoryController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _hostEnvironment = environment;
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
            if (ModelState.IsValid)
            {
                if (GraphicGalleryFiles != null)
                {
                    ImageFileUpload imageFileUpload = new(_hostEnvironment);
                    string folder = "gallery\\";
                    history.GraphicGallery = new List<Graphic>();

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
                            history.GraphicGallery.Add(gallery);
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
                    string folder = "gallery\\";

                    history.GraphicGallery = new List<Graphic>();

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

                            history.GraphicGallery.Add(gallery);

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
        [HttpDelete]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirm(int id)
        {
            if (_context.History == null)
            {
                return Problem("Entity set 'ApplicationDbContext.History'  is null.");
            }
            var history = await _context.History.FindAsync(id);
            if (history != null)
            {
                var ImagesToDelete = _context.GraphicGallery.Where(u => u.Id == id).ToList();
                foreach (var image in ImagesToDelete)
                {
                    var DeleteImagePath = Path.Combine(_hostEnvironment.WebRootPath + image.URL);
                    var DeleteMiniImagePath = Path.Combine(_hostEnvironment.WebRootPath + "\\miniGallery" + image.URL);
                    DeleteImage(DeleteImagePath);
                    DeleteImage(DeleteMiniImagePath);
                }
                var HistoryGalleriesItemToDelete = _context.GraphicGallery.Where(i => i.HistoryId == id).ToList();
                foreach (var item in HistoryGalleriesItemToDelete)
                {
                    _context.GraphicGallery.Remove(item);
                }
                _context.History.Remove(history);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult List()
        {
            var HistoryTexts = _context.History.Include(t => t.GraphicGallery).ToList();
            return View(HistoryTexts);

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
