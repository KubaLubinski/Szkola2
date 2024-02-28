using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Szkola2.Data;
using Szkola2.Infrastructure;
using Szkola2.Models;

namespace Szkola2.Controllers
{
    public class DocumentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private IWebHostEnvironment _hostEnvironment;

        public DocumentsController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _hostEnvironment = environment;
        }


        // GET: Documents
        
        public async Task<IActionResult> Index()
        {
              return _context.Documents != null ? 
                          View(await _context.Documents.ToListAsync()) :
                          Problem("Entity set 'ApplicationDbContext.Documents'  is null.");
        }

        // GET: Documents/Details/5
        [Authorize(Roles = "admin,dyrektor,nauczyciel,uczen")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Documents == null)
            {
                return NotFound();
            }

            var document = await _context.Documents
                .FirstOrDefaultAsync(m => m.DocumentId == id);
            if (document == null)
            {
                return NotFound();
            }

            return View(document);
        }

        // GET: Documents/Create
        [Authorize(Roles = "admin,dyrektor,nauczyciel,uczen")]

        public IActionResult Create()
        {
            return View();
        }

        // POST: Documents/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "admin,dyrektor,nauczyciel,uczen")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DocumentId,DocumentDownload,DocumentName,DocumentType")] Document document, IFormFile documents)
        {
            if (ModelState.IsValid)
            {
                if ( document!= null)
                {
                    DocumentFileUpload documentFileResult = new(_hostEnvironment);
                    FileSendResult fileSendResult = documentFileResult.DocumentSend(documents, "documents");
                    if(fileSendResult.Success)
                    {
                        document.DocumentName = fileSendResult.Name;

                        string pobierz = Url.Action("Download", "Documents", new { fileName = document.DocumentName }, Request.Scheme);
                        
                        ViewData["LinkURL"] = pobierz;
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Wybrany plik nie jest żądanym typem tekstowym. Można przesyłać pliki z rozszerzeniami .pdf .docx .doc .txt";
                        ViewData["DocumentId"] = new SelectList(_context.Documents, "DocumentId", "DocumentName", document.DocumentId);
                        return View(document);
                    }
                }


                _context.Add(document);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(document);
        }

        // GET: Documents/Edit/5
        [Authorize(Roles = "admin,dyrektor,nauczyciel")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Documents == null)
            {
                return NotFound();
            }

            var document = await _context.Documents.FindAsync(id);
            if (document == null)
            {
                return NotFound();
            }
            return View(document);
        }

        // POST: Documents/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "admin,dyrektor,nauczyciel")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,IFormFile newDocumentFile, [Bind("DocumentId,DocumentDownload,DocumentName,DocumentType")] Document document)
        {
            if (id != document.DocumentId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if(newDocumentFile != null && newDocumentFile.Length >0 )
                    {
                        DocumentFileUpload documentFileResult = new(_hostEnvironment);
                        FileSendResult fileSendResult = documentFileResult.DocumentSend(newDocumentFile, "documents");
                        if(fileSendResult.Success)
                        {
                            document.DocumentName = fileSendResult.Name;
                        }
                        else
                        {
                            ViewBag.ErrorMessage = "Wybrany plik nie jest żądanym typem tekstowym. Mozna przesyłać pliki z rozszerzeniami .pdf .docx .doc .txt";
                            return View(document);
                        }
                    }
                    _context.Update(document);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DocumentExists(document.DocumentId))
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
            return View(document);
        }

        // GET: Documents/Delete/5
        [Authorize(Roles = "admin,dyrektor")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Documents == null)
            {
                return NotFound();
            }

            var document = await _context.Documents
                .FirstOrDefaultAsync(m => m.DocumentId == id);
            if (document == null)
            {
                return NotFound();
            }

            return View(document);
        }

        // POST: Documents/Delete/5
        [Authorize(Roles = "admin,dyrektor")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Documents == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Documents'  is null.");
            }
            var document = await _context.Documents.FindAsync(id);
            if (document != null)
            {
                _context.Documents.Remove(document);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DocumentExists(int id)
        {
          return (_context.Documents?.Any(e => e.DocumentId == id)).GetValueOrDefault();
        }
    }
}
