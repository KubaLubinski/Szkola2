using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Szkola2.Data;
using Szkola2.Models;

namespace Szkola2.Infrastructure
{
    public class GalleryUploadResult
    {
        public bool Success { get; set; } = true;
        public string? Error { get; set; }
        public List<Graphic> Items { get; } = new();
    }

    public class GraphicDeleteResult
    {
        public bool Found { get; set; }
        public bool Allowed { get; set; }
        /// <summary>true, gdy użytkownik nie ma uprawnień do zdjęć tego wpisu (np. nauczyciel i cudzy tekst).</summary>
        public bool Forbidden { get; set; }
        public int? TextId { get; set; }
        public int? EventId { get; set; }
    }

    /// <summary>Wspólna obsługa galerii zdjęć dla tekstów i wpisów kalendarium.</summary>
    public class GalleryService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ApplicationDbContext _db;
        private readonly IDemoContext _demo;
        private readonly FileCleaner _files;
        private readonly ILogger<GalleryService> _logger;

        public GalleryService(IWebHostEnvironment env, ApplicationDbContext db, IDemoContext demo, FileCleaner files,
            ILogger<GalleryService> logger)
        {
            _env = env;
            _db = db;
            _demo = demo;
            _files = files;
            _logger = logger;
        }

        /// <summary>Zapisuje pliki na dysku (oryginał + miniatura). Przy błędzie sprząta to, co zdążył zapisać.</summary>
        public GalleryUploadResult Save(IFormFileCollection? files)
        {
            var result = new GalleryUploadResult();
            if (files == null) return result;

            var upload = new ImageFileUpload(_env);

            foreach (var file in files)
            {
                if (file.Length == 0) continue;

                // sprawdza typ i zawartość, zmniejsza zdjęcie (max 1920 px), kasuje metadane i tworzy miniaturę
                if (!upload.TryUploadGalleryImage(file, out var url, out var error))
                {
                    _logger.LogWarning("Odrzucono plik galerii ({Length} B): {Error}", file.Length, error);
                    Discard(result);
                    return new GalleryUploadResult { Success = false, Error = error };
                }

                result.Items.Add(new Graphic { Name = Path.GetFileName(file.FileName), URL = url });
            }
            return result;
        }

        /// <summary>Kasuje z dysku pliki zapisane przez Save (gdy dalsza operacja się nie powiodła).</summary>
        public void Discard(GalleryUploadResult result)
        {
            foreach (var g in result.Items) _files.DeleteGalleryImage(g.URL);
            result.Items.Clear();
        }

        /// <summary>
        /// Usuwa jedno zdjęcie z galerii tekstu lub wydarzenia. Kontrole:
        /// tekst – autor albo admin/dyrektor; wydarzenie – tylko admin/dyrektor; konto demo – tylko własne wpisy.
        /// (Wcześniej każdy nauczyciel mógł usunąć dowolne zdjęcie, znając jego numer.)
        /// </summary>
        public async Task<GraphicDeleteResult> DeleteOneAsync(int id, ClaimsPrincipal user)
        {
            var g = await _db.GraphicGallery
                .Include(x => x.Texts)
                .Include(x => x.PastEvent)
                .FirstOrDefaultAsync(x => x.Id == id);

            // rodzic niewidoczny dla bieżącego użytkownika (np. cudzy wpis demo) = traktujemy jak brak zdjęcia
            if (g == null
                || (g.TextId != null && g.Texts == null)
                || (g.EventId != null && g.PastEvent == null))
            {
                return new GraphicDeleteResult { Found = false };
            }

            var result = new GraphicDeleteResult { Found = true, TextId = g.TextId, EventId = g.EventId };

            bool isManager = user.IsInRole("admin") || user.IsInRole("dyrektor");
            bool isTextAuthor = g.Texts != null && g.Texts.Id == user.FindFirstValue(ClaimTypes.NameIdentifier);
            bool mayManage = g.Texts != null ? (isManager || isTextAuthor) : isManager;
            if (!mayManage)
            {
                _logger.LogWarning("Odmowa usunięcia zdjęcia {GraphicId} przez {UserId}", id, user.FindFirstValue(ClaimTypes.NameIdentifier));
                result.Forbidden = true;
                return result;
            }

            IDemoOwned? parent = (IDemoOwned?)g.Texts ?? g.PastEvent;
            result.Allowed = !_demo.IsDemo || (parent != null && _demo.CanModify(parent));
            if (!result.Allowed) return result;

            var url = g.URL;
            _db.GraphicGallery.Remove(g);
            await _db.SaveChangesAsync();
            _files.DeleteGalleryImage(url);
            _logger.LogInformation("Usunięto zdjęcie {GraphicId} (użytkownik {UserId})", id, user.FindFirstValue(ClaimTypes.NameIdentifier));
            return result;
        }
    }
}
