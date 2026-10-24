namespace Szkola2.Infrastructure
{
    /// <summary>
    /// Usuwanie plików wgranych przez aplikację. Ścieżki odpowiadają temu, co zapisują
    /// ImageFileUpload (img, img\mini, gallery, miniGallery) i DocumentFileUpload (documents).
    /// </summary>
    public class FileCleaner
    {
        private readonly string _root;
        private readonly ILogger<FileCleaner> _logger;

        public FileCleaner(IWebHostEnvironment env, ILogger<FileCleaner> logger)
        {
            _root = Path.GetFullPath(env.WebRootPath);
            _logger = logger;
        }

        public void DeleteMainGraphic(string? name)
        {
            if (string.IsNullOrWhiteSpace(name)) return;
            Delete(Path.Combine(_root, "img", name));
            Delete(Path.Combine(_root, "img", "mini", name));
        }

        /// <param name="url">Wartość Graphic.URL, np. "/gallery/guid_nazwa.jpg" (starsze wpisy: "\gallery\guid_nazwa.jpg")</param>
        public void DeleteGalleryImage(string? url)
        {
            if (string.IsNullOrWhiteSpace(url)) return;
            var relative = url.Replace('\\', '/').TrimStart('/'); // stare wpisy mają "\gallery\...", nowe "/gallery/..."
            Delete(Path.Combine(_root, relative));
            Delete(Path.Combine(_root, "miniGallery", relative));
        }

        /// <summary>ZAŁOŻENIE: DocumentFileUpload zapisuje do wwwroot\documents\{Name}. Do potwierdzenia.</summary>
        public void DeleteDocument(string? name)
        {
            if (string.IsNullOrWhiteSpace(name)) return;
            Delete(Path.Combine(_root, "documents", name));
        }

        private void Delete(string path)
        {
            try
            {
                var full = Path.GetFullPath(path);
                // zabezpieczenie przed wyjściem poza wwwroot
                if (!full.StartsWith(_root, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Odrzucono ścieżkę poza wwwroot przy usuwaniu pliku: {Path}", path);
                    return;
                }
                if (File.Exists(full)) File.Delete(full);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                // plik zajęty lub brak uprawnień – zostaje na dysku, ale zostawiamy ślad
                _logger.LogWarning(ex, "Nie udało się usunąć pliku {Path}", path);
            }
        }
    }
}
