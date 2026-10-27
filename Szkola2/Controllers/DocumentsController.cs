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
    public class DocumentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly IDemoContext _demo;
        private readonly FileCleaner _files;
        private readonly ILogger<DocumentsController> _logger;

        public DocumentsController(ApplicationDbContext context, IWebHostEnvironment environment,
            IDemoContext demo, FileCleaner files, ILogger<DocumentsController> logger)
        {
            _context = context;
            _hostEnvironment = environment;
            _demo = demo;
            _files = files;
            _logger = logger;
        }

        // GET: Documents
        public async Task<IActionResult> Index()
        {
            return _context.Documents != null ?
                        View(await _context.Documents.ToListAsync()) :
                        Problem("Entity set 'ApplicationDbContext.Documents'  is null.");
        }

        // GET: Documents/Details/5
        [Authorize(Roles = "admin,dyrektor,nauczyciel,uczen,rodzic")]
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
        [Authorize(Roles = "admin,dyrektor,nauczyciel")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Documents/Create
        [Authorize(Roles = "admin,dyrektor,nauczyciel")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DocumentDownload,DocumentName,DocumentType,Content")] Document document, IFormFile? documents)
        {
            document.Content = HtmlCleaner.Clean(document.Content);
            if (!ModelState.IsValid)
            {
                return View(document);
            }

            if (document.DocumentDownload == null && document.DocumentName == null)
            {
                return CreateFail(document, "Należy podać nazwe dokumentu, lub dokument nie został załadowany");
            }

            if (documents == null || documents.Length == 0)
            {
                return CreateFail(document, "Należy wybrać plik dokumentu.");
            }

            if (_demo.IsDemo)
            {
                if (await _context.Documents.CountAsync(d => d.DemoOwnerId == _demo.UserId) >= DemoLimits.MaxDocuments)
                    return CreateFail(document, $"Konto testowe może mieć maksymalnie {DemoLimits.MaxDocuments} własnych dokumentów. Usuń któryś, aby dodać nowy.");

                if (documents.Length > DemoLimits.MaxFileBytes)
                    return CreateFail(document, $"Konto testowe: maksymalny rozmiar pliku to {DemoLimits.MaxFileBytes / 1024 / 1024} MB.");

                var extension = Path.GetExtension(documents.FileName).ToLowerInvariant();
                if (!DemoLimits.DocumentExtensions.Contains(extension))
                    return CreateFail(document, "Wybrany plik nie jest żądanym typem tekstowym. Można przesyłać pliki z rozszerzeniami .pdf .docx .doc .txt");
            }

            string? savedName = null;
            try
            {
                DocumentFileUpload documentFileResult = new(_hostEnvironment);
                FileSendResult fileSendResult = documentFileResult.DocumentSend(documents, "documents");
                if (!fileSendResult.Success)
                {
                    return CreateFail(document, "Wybrany plik nie jest żądanym typem tekstowym. Można przesyłać pliki z rozszerzeniami .pdf .docx .doc .txt");
                }

                savedName = fileSendResult.Name;
                document.DocumentName = savedName;

                _context.Add(document);
                await _context.SaveChangesAsync();
            }
            catch
            {
                _files.DeleteDocument(savedName);
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Documents/Edit/5
        [Authorize(Roles = "admin,dyrektor,nauczyciel")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Documents == null)
            {
                return NotFound();
            }

            var document = await _context.Documents.FirstOrDefaultAsync(d => d.DocumentId == id);
            if (document == null)
            {
                return NotFound();
            }

            if (_demo.IsDemo && !_demo.CanModify(document))
            {
                TempData[DemoMessages.Key] = DemoMessages.ViewOnly;
            }
            return View(document);
        }

        // POST: Documents/Edit/5
        [Authorize(Roles = "admin,dyrektor,nauczyciel")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, IFormFile? newDocumentFile, [Bind("DocumentId,DocumentDownload,DocumentName,DocumentType,Content")] Document document)
        {
            document.Content = HtmlCleaner.Clean(document.Content);
            if (id != document.DocumentId)
            {
                return NotFound();
            }

            var entity = await _context.Documents.FirstOrDefaultAsync(d => d.DocumentId == id);
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
                document.DocumentName = entity.DocumentName;
                return View(document);
            }

            if (document.DocumentDownload == null && entity.DocumentName == null)
            {
                document.DocumentName = entity.DocumentName;
                ViewBag.ErrorMessage = "Należy podać nazwe dokumentu, lub dokument nie został załadowany";
                return View(document);
            }

            var hasNewFile = newDocumentFile != null && newDocumentFile.Length > 0;
            if (hasNewFile && _demo.IsDemo)
            {
                var extension = Path.GetExtension(newDocumentFile!.FileName).ToLowerInvariant();
                if (newDocumentFile.Length > DemoLimits.MaxFileBytes || !DemoLimits.DocumentExtensions.Contains(extension))
                {
                    document.DocumentName = entity.DocumentName;
                    ViewBag.ErrorMessage = $"Konto testowe: pliki .pdf .docx .doc .txt, maksymalnie {DemoLimits.MaxFileBytes / 1024 / 1024} MB.";
                    return View(document);
                }
            }

            string? newName = null;
            var oldName = entity.DocumentName;
            try
            {
                if (hasNewFile)
                {
                    DocumentFileUpload documentFileResult = new(_hostEnvironment);
                    FileSendResult fileSendResult = documentFileResult.DocumentSend(newDocumentFile!, "documents");
                    if (!fileSendResult.Success)
                    {
                        document.DocumentName = entity.DocumentName;
                        ViewBag.ErrorMessage = "Wybrany plik nie jest żądanym typem tekstowym. Mozna przesyłać pliki z rozszerzeniami .pdf .docx .doc .txt";
                        return View(document);
                    }
                    newName = fileSendResult.Name;
                    entity.DocumentName = newName;
                }

                entity.DocumentDownload = document.DocumentDownload;
                entity.DocumentType = document.DocumentType;
                entity.Content = document.Content;

                await _context.SaveChangesAsync();
            }
            catch
            {
                _files.DeleteDocument(newName);
                throw;
            }

            if (newName != null) _files.DeleteDocument(oldName);

            return RedirectToAction(nameof(Index));
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

            var document = await _context.Documents.FirstOrDefaultAsync(d => d.DocumentId == id);
            if (document == null)
            {
                return RedirectToAction(nameof(Index));
            }

            if (!_demo.CanModify(document))
            {
                TempData[DemoMessages.Key] = DemoMessages.ReadOnly;
                return RedirectToAction(nameof(Index));
            }

            var fileName = document.DocumentName;
            _context.Documents.Remove(document);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Usunięto dokument {DocumentId} (użytkownik {UserId})", id, User.FindFirstValue(ClaimTypes.NameIdentifier));
            _files.DeleteDocument(fileName);

            return RedirectToAction(nameof(Index));
        }

        private IActionResult CreateFail(Document document, string message)
        {
            ViewBag.ErrorMessage = message;
            return View("Create", document);
        }
    }
}
