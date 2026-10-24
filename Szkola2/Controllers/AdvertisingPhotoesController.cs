using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Szkola2.Data;
using Szkola2.Infrastructure;
using Szkola2.Models;

namespace Szkola2.Controllers
{
    public class AdvertisingPhotoesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly IDemoContext _demo;
        private readonly FileCleaner _files;
        private readonly ILogger<AdvertisingPhotoesController> _logger;

        public AdvertisingPhotoesController(ApplicationDbContext context, IWebHostEnvironment environment,
            IDemoContext demo, FileCleaner files, ILogger<AdvertisingPhotoesController> logger)
        {
            _context = context;
            _hostEnvironment = environment;
            _demo = demo;
            _files = files;
            _logger = logger;
        }

        // GET: AdvertisingPhotoes
        [Authorize(Roles = "admin,dyrektor")]
        public async Task<IActionResult> Index()
        {
              return _context.AdvertisingPhotos != null ? 
                          View(await _context.AdvertisingPhotos.ToListAsync()) :
                          Problem("Entity set 'ApplicationDbContext.AdvertisingPhotos'  is null.");
        }

        // GET: AdvertisingPhotoes/Details/5
        [Authorize(Roles = "admin,dyrektor")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.AdvertisingPhotos == null)
            {
                return NotFound();
            }

            var advertisingPhoto = await _context.AdvertisingPhotos
                
                .FirstOrDefaultAsync(m => m.PhotoId == id);
            if (advertisingPhoto == null)
            {
                return NotFound();
            }

            return View(advertisingPhoto);
        }

        // GET: AdvertisingPhotoes/Create
        [Authorize(Roles = "admin,dyrektor")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: AdvertisingPhotoes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "admin,dyrektor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PhotoId,Photo")] AdvertisingPhoto advertisingPhoto, IFormFile photo)
        {
            // konto testowe nie może zmieniać slidera; blokada musi być PRZED zapisem pliku,
            // bo inaczej wgrywane zdjęcia zostawałyby na dysku mimo odrzuconego zapisu w bazie
            if (_demo.IsDemo)
            {
                TempData[DemoMessages.Key] = DemoMessages.ReadOnly;
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                if(photo != null && photo.Length >0)
                {
                    ImageFileUpload imageFileResult = new(_hostEnvironment);
                    FileSendResult fileSendResult = imageFileResult.SendFile(photo, "img", 600);
                    if(fileSendResult.Success)
                    {
                        advertisingPhoto.Photo = fileSendResult.Name;
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "wybrany plik nie jest obrazkiem, lub zawiera złe rozszerzenie!";
                        return View(advertisingPhoto);
                    }
                }
                _context.Add(advertisingPhoto);
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch
                {
                    _files.DeleteMainGraphic(advertisingPhoto.Photo);
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(advertisingPhoto);
        }

        // GET: AdvertisingPhotoes/Edit/5
        [Authorize(Roles = "admin,dyrektor")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.AdvertisingPhotos == null)
            {
                return NotFound();
            }

            var advertisingPhoto = await _context.AdvertisingPhotos.FindAsync(id);
            if (advertisingPhoto == null)
            {
                return NotFound();
            }
            // advertisingPhoto.Photo = _context.AdvertisingPhotos.ToList(); <-- nie wiem jak
            
            return View(advertisingPhoto);
        }

        // POST: AdvertisingPhotoes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "admin,dyrektor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PhotoId,Photo")] AdvertisingPhoto advertisingPhoto)
        {
            if (id != advertisingPhoto.PhotoId)
            {
                return NotFound();
            }

            if (_demo.IsDemo)
            {
                TempData[DemoMessages.Key] = DemoMessages.ReadOnly;
                return RedirectToAction(nameof(Index));
            }

            // nazwa pliku musi wskazywać istniejący plik w wwwroot/img (bez ścieżek typu ../)
            var photoName = advertisingPhoto.Photo;
            if (string.IsNullOrWhiteSpace(photoName)
                || photoName != Path.GetFileName(photoName)
                || !System.IO.File.Exists(Path.Combine(_hostEnvironment.WebRootPath, "img", photoName)))
            {
                ModelState.AddModelError(nameof(AdvertisingPhoto.Photo), "Wskazany plik zdjęcia nie istnieje.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(advertisingPhoto);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AdvertisingPhotoExists(advertisingPhoto.PhotoId))
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
            return View(advertisingPhoto);
        }

        // GET: AdvertisingPhotoes/Delete/5
        [Authorize(Roles = "admin,dyrektor")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.AdvertisingPhotos == null)
            {
                return NotFound();
            }

            var advertisingPhoto = await _context.AdvertisingPhotos
                .FirstOrDefaultAsync(m => m.PhotoId == id);
            if (advertisingPhoto == null)
            {
                return NotFound();
            }

            return View(advertisingPhoto);
        }

        // POST: AdvertisingPhotoes/Delete/5
        [Authorize(Roles = "admin,dyrektor")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.AdvertisingPhotos == null)
            {
                return Problem("Entity set 'ApplicationDbContext.AdvertisingPhotos'  is null.");
            }
            if (_demo.IsDemo)
            {
                TempData[DemoMessages.Key] = DemoMessages.ReadOnly;
                return RedirectToAction(nameof(Index));
            }

            var advertisingPhoto = await _context.AdvertisingPhotos.FindAsync(id);
            if (advertisingPhoto != null)
            {
                var fileName = advertisingPhoto.Photo;
                _context.AdvertisingPhotos.Remove(advertisingPhoto);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Usunięto zdjęcie slidera {PhotoId} (użytkownik {UserId})", id, User.FindFirstValue(ClaimTypes.NameIdentifier));
                // plik kasujemy dopiero po udanym zapisie w bazie (wcześniej zostawał na dysku)
                _files.DeleteMainGraphic(fileName);
            }

            return RedirectToAction(nameof(Index));
        }

        private bool AdvertisingPhotoExists(int id)
        {
          return (_context.AdvertisingPhotos?.Any(e => e.PhotoId == id)).GetValueOrDefault();
        }
    }
}
