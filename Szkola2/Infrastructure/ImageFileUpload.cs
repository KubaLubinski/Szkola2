using System.Text.RegularExpressions;

namespace Szkola2.Infrastructure
{
    /// <summary>
    /// Zapis zdjęć wgrywanych przez formularze. Każde zdjęcie jest sprawdzane (czy to naprawdę obraz),
    /// zmniejszane do max 1920 px szerokości, pozbawiane metadanych i zapisywane razem z miniaturą.
    /// </summary>
    public class ImageFileUpload
    {
        private const string InvalidImageMessage = "Wybrany plik nie jest poprawnym obrazkiem.";

        private readonly IWebHostEnvironment hostingEnvironment;

        public ImageFileUpload(IWebHostEnvironment environment)
        {
            hostingEnvironment = environment;
        }

        public bool FileTypeCheck(string extension)
        {
            return extension.ToLower() switch
            {
                ".jpg" or ".png" or ".gif" => true,
                _ => false,
            };
        }

        /// <summary>Oryginał do wwwroot/{destination}, miniatura (wysokość {height}) do wwwroot/{destination}/mini.</summary>
        public FileSendResult SendFile(IFormFile picture, string destination, int height)
            => Send(picture, destination, "mini", height);

        /// <summary>Jak SendFile, ale miniatura trafia do wwwroot/{destination}/zdjecia.</summary>
        public FileSendResult SendPhoto(IFormFile photo, string destination, int height)
            => Send(photo, destination, "zdjecia", height);

        private FileSendResult Send(IFormFile picture, string destination, string miniFolder, int height)
        {
            var result = new FileSendResult();

            string extension = Path.GetExtension(picture.FileName).ToLowerInvariant();
            if (!FileTypeCheck(extension))
            {
                result.Name = Path.GetFileName(picture.FileName);
                result.Success = false;
                result.Error = "Niepoprawny typ pliku graficznego.";
                return result;
            }

            result.Name = Guid.NewGuid().ToString() + extension;

            var root = hostingEnvironment.WebRootPath;
            var originalPath = Path.Combine(root, destination, result.Name);
            var miniPath = Path.Combine(root, destination, miniFolder, result.Name);

            try
            {
                using var input = ImageProcessing.ReadToMemory(picture);
                ImageProcessing.SaveOriginalAndMini(input, originalPath, miniPath, height);
                result.Success = true;
            }
            catch (InvalidDataException ex)
            {
                DeleteQuietly(originalPath);
                DeleteQuietly(miniPath);
                result.Success = false;
                result.Error = ex.Message;
            }

            return result;
        }

        /// <summary>
        /// Zdjęcie do galerii: oryginał do wwwroot/gallery, miniatura do wwwroot/miniGallery/gallery.
        /// Zwraca adres w formie "/gallery/{plik}" (widoki dopisują go do "~/miniGallery/").
        /// </summary>
        public bool TryUploadGalleryImage(IFormFile file, out string? url, out string? error)
        {
            url = null;
            error = null;

            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!FileTypeCheck(extension))
            {
                error = "Jeden z wybranych plików nie jest obrazkiem!";
                return false;
            }

            string fileName = Guid.NewGuid() + "_" + SafeStem(file.FileName) + extension;
            var originalPath = Path.Combine(hostingEnvironment.WebRootPath, "gallery", fileName);
            var miniPath = Path.Combine(hostingEnvironment.WebRootPath, "miniGallery", "gallery", fileName);

            try
            {
                using var input = ImageProcessing.ReadToMemory(file);
                ImageProcessing.SaveOriginalAndMini(input, originalPath, miniPath);
            }
            catch (InvalidDataException)
            {
                DeleteQuietly(originalPath);
                DeleteQuietly(miniPath);
                error = "Jeden z wybranych plików nie jest obrazkiem!";
                return false;
            }

            url = "/gallery/" + fileName;
            return true;
        }

        /// <summary>Bezpieczna część nazwy pliku: tylko litery, cyfry, '_' i '-' (bez ścieżek i znaków specjalnych).</summary>
        private static string SafeStem(string originalName)
        {
            var last = originalName.Replace('\\', '/').Split('/').Last();
            var stem = Path.GetFileNameWithoutExtension(last);
            stem = Regex.Replace(stem, "[^A-Za-z0-9_-]", "_");
            if (stem.Length > 60) stem = stem[..60];
            return string.IsNullOrEmpty(stem) ? "zdjecie" : stem;
        }

        private static void DeleteQuietly(string path)
        {
            try { if (File.Exists(path)) File.Delete(path); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
    }
}
