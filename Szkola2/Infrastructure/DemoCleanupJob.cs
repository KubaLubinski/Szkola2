using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Szkola2.Data;
using Szkola2.Models;

namespace Szkola2.Infrastructure
{
    /// <summary>
    /// Job Hangfire (co 10 min): kasuje wygasłe konta demo razem z ich treściami, zdjęciami i dokumentami.
    /// Działa bez zalogowanego użytkownika, więc filtry widoczności trzeba wyłączyć (IgnoreQueryFilters).
    /// </summary>
    public class DemoCleanupJob
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<AppUser> _users;
        private readonly FileCleaner _files;
        private readonly ILogger<DemoCleanupJob> _log;

        public DemoCleanupJob(ApplicationDbContext db, UserManager<AppUser> users, FileCleaner files, ILogger<DemoCleanupJob> log)
        {
            _db = db;
            _users = users;
            _files = files;
            _log = log;
        }

        public async Task RunAsync()
        {
            var now = DateTime.UtcNow;
            var expired = await _db.AppUsers!
                .Where(u => u.IsDemo && u.DemoExpiresAt != null && u.DemoExpiresAt < now)
                .Take(50)
                .ToListAsync();

            int removed = 0;
            foreach (var user in expired)
            {
                try
                {
                    await DeleteAccountAsync(user);
                    removed++;
                }
                catch (Exception ex)
                {
                    _log.LogError(ex, "Nie udało się usunąć konta demo {UserId}", user.Id);
                    _db.ChangeTracker.Clear();
                }
            }

            if (expired.Count > 0)
                _log.LogInformation("Sprzątanie kont demo: usunięto {Removed} z {Expired} wygasłych", removed, expired.Count);
        }

        private async Task DeleteAccountAsync(AppUser user)
        {
            var mainGraphics = new List<string?>();
            var galleryUrls = new List<string?>();
            var documentFiles = new List<string?>();

            var texts = await _db.Texts!.IgnoreQueryFilters()
                .Include(t => t.GraphicGallery)
                .Where(t => t.DemoOwnerId == user.Id)
                .ToListAsync();
            foreach (var t in texts)
            {
                mainGraphics.Add(t.MainGraphic);
                if (t.GraphicGallery != null)
                {
                    galleryUrls.AddRange(t.GraphicGallery.Select(g => g.URL));
                    _db.GraphicGallery!.RemoveRange(t.GraphicGallery);
                }
            }
            _db.Texts!.RemoveRange(texts);

            var events = await _db.PastSchoolEvents!.IgnoreQueryFilters()
                .Include(e => e.GraphicGallery)
                .Where(e => e.DemoOwnerId == user.Id)
                .ToListAsync();
            foreach (var e in events)
            {
                mainGraphics.Add(e.MainGraphic);
                if (e.GraphicGallery != null)
                {
                    galleryUrls.AddRange(e.GraphicGallery.Select(g => g.URL));
                    _db.GraphicGallery!.RemoveRange(e.GraphicGallery);
                }
            }
            _db.PastSchoolEvents!.RemoveRange(events);

            var docs = await _db.Documents!.IgnoreQueryFilters()
                .Where(d => d.DemoOwnerId == user.Id)
                .ToListAsync();
            documentFiles.AddRange(docs.Select(d => d.DocumentName));
            _db.Documents!.RemoveRange(docs);

            await _db.SaveChangesAsync();

            var result = await _users.DeleteAsync(user);
            if (!result.Succeeded)
                throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));

            // pliki kasujemy dopiero, gdy baza została posprzątana
            foreach (var name in mainGraphics) _files.DeleteMainGraphic(name);
            foreach (var url in galleryUrls) _files.DeleteGalleryImage(url);
            foreach (var name in documentFiles) _files.DeleteDocument(name);
        }
    }
}
