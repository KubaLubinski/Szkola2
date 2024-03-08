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
                .Include(t => t.HistoryGallerys)
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
        public async Task<IActionResult> Create([Bind("Id,Year,Content")] History history, IFormFileCollection? HistoryGalleryFiles)
        {
            if (ModelState.IsValid)
            {
                //_context.Add(history);
                //await _context.SaveChangesAsync();
                //return RedirectToAction(nameof(Index));

                if (HistoryGalleryFiles != null)
                {
                    ImageFileUpload imageFileUpload = new(_hostEnvironment);
                    string folder = "gallery\\";

                    history.HistoryGallerys = new List<HistoryGallery>();

                    foreach (var file in HistoryGalleryFiles)
                    {
                        var gallery = new HistoryGallery()
                        {
                            Name = file.FileName,
                            URL = imageFileUpload.UploadImage(folder, file)
                        };


                        history.HistoryGallerys.Add(gallery);

                        string path = Path.Combine(_hostEnvironment.WebRootPath + gallery.URL);

                        using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                        {
                            using (Image imgFile = Image.FromStream(fs))
                            {
                                var miniFile = imgFile.ScaleByWidth(800);
                                var upload = Path.Combine(_hostEnvironment.WebRootPath + "\\miniGallery\\" + gallery.URL);
                                miniFile.SaveAs(upload);
                            }
                        }
;
                    }
                }

                if (!YearExist(history.Year))
                {
                    _context.Add(history);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ViewBag.ErrorMessage = "Wpis o takim roku już istnieje!";
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

            history.HistoryGallerys = _context.HistoryGalleries.Where(g => g.HistoryId == id).ToList();
            return View(history);
        }

        // POST: History/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "admin,dyrektor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Year,Content")] History history, IFormFileCollection? HistoryGalleryFiles)
        {
            if (id != history.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (HistoryGalleryFiles != null)
                    {
                        ImageFileUpload imageFileUpload = new(_hostEnvironment);
                        string folder = "gallery\\";

                        history.HistoryGallerys = new List<HistoryGallery>();

                        foreach (var file in HistoryGalleryFiles)
                        {
                            var gallery = new HistoryGallery()
                            {
                                Name = file.FileName,
                                URL = imageFileUpload.UploadImage(folder, file)
                            };


                            history.HistoryGallerys.Add(gallery);

                            string path = Path.Combine(_hostEnvironment.WebRootPath + gallery.URL);

                            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                            {
                                using (Image imgFile = Image.FromStream(fs))
                                {
                                    var miniFile = imgFile.ScaleByWidth(800);
                                    var upload = Path.Combine(_hostEnvironment.WebRootPath + "\\miniGallery\\" + gallery.URL);
                                    miniFile.SaveAs(upload);
                                }
                            }
        ;
                        }
                    }
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



            /*if (history.HistoryGalleryFiles != null)
            {
                foreach (var file in history.HistoryGalleryFiles)
                {
                    string tempFileName = file.FileName;
                    if (!tempFileName.Contains)
                }
            }*/


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
                var ImagesToDelete = _context.HistoryGalleries.Where(u => u.Id == id).ToList();
                foreach (var image in ImagesToDelete)
                {
                    var DeleteImagePath = Path.Combine(_hostEnvironment.WebRootPath + image.URL);
                    var DeleteMiniImagePath = Path.Combine(_hostEnvironment.WebRootPath + "\\miniGallery" + image.URL);
                    DeleteImage(DeleteImagePath);
                    DeleteImage(DeleteMiniImagePath);
                }
                var HistoryGalleriesItemToDelete = _context.HistoryGalleries.Where(i => i.HistoryId == id).ToList();
                foreach (var item in HistoryGalleriesItemToDelete)
                {
                    _context.HistoryGalleries.Remove(item);
                }
                _context.History.Remove(history);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult DeleteOneImage(int Id)
        {
            var ImageToDelete = _context.HistoryGalleries.FirstOrDefault(u => u.Id == Id);
            var DeleteImagePath = Path.Combine(_hostEnvironment.WebRootPath + ImageToDelete.URL);
            var DeleteMiniImagePath = Path.Combine(_hostEnvironment.WebRootPath + "\\miniGallery" + ImageToDelete.URL);
            DeleteImage(DeleteImagePath);
            DeleteImage(DeleteMiniImagePath);

            _context.HistoryGalleries.Remove(ImageToDelete);
            _context.SaveChanges();
            return RedirectToAction(nameof(Edit));
        }
        private void DeleteImage(string deleteImagePath)
        {
            bool test = System.IO.File.Exists(deleteImagePath);
            if (System.IO.File.Exists(deleteImagePath))
            {
                string tak = "dziala";
                System.IO.File.Delete(deleteImagePath);
            }
        }

        private bool HistoryExists(int id)
        {
            return (_context.History?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        private bool YearExist(int? year)
        {
            return (_context.History?.Any(e => e.Year == year)).GetValueOrDefault();
        }
    }
}
