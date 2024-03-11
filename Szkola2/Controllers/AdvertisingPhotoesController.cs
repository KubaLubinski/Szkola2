using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        private IWebHostEnvironment _hostEnvironment;

        public AdvertisingPhotoesController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _hostEnvironment = environment;
        }

        // GET: AdvertisingPhotoes
        public async Task<IActionResult> Index()
        {
              return _context.AdvertisingPhotos != null ? 
                          View(await _context.AdvertisingPhotos.ToListAsync()) :
                          Problem("Entity set 'ApplicationDbContext.AdvertisingPhotos'  is null.");
        }

        // GET: AdvertisingPhotoes/Details/5
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
        public IActionResult Create()
        {
            return View();
        }

        // POST: AdvertisingPhotoes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PhotoId,Photo")] AdvertisingPhoto advertisingPhoto, IFormFile photo)
        {
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
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(advertisingPhoto);
        }

        // GET: AdvertisingPhotoes/Edit/5
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PhotoId,Photo")] AdvertisingPhoto advertisingPhoto)
        {
            if (id != advertisingPhoto.PhotoId)
            {
                return NotFound();
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
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.AdvertisingPhotos == null)
            {
                return Problem("Entity set 'ApplicationDbContext.AdvertisingPhotos'  is null.");
            }
            var advertisingPhoto = await _context.AdvertisingPhotos.FindAsync(id);
            if (advertisingPhoto != null)
            {
                _context.AdvertisingPhotos.Remove(advertisingPhoto);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AdvertisingPhotoExists(int id)
        {
          return (_context.AdvertisingPhotos?.Any(e => e.PhotoId == id)).GetValueOrDefault();
        }
    }
}
