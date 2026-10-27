using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace Szkola2.Infrastructure
{
    /// <summary>Obróbka zdjęć (ImageSharp): weryfikacja, obrót wg EXIF, usunięcie metadanych, zmniejszenie.</summary>
    public static class ImageProcessing
    {
        public const int MaxWidth = 1920;
        public const int MiniHeight = 480; // karty na stronie mają 160–380 px wysokości
        public const int MiniWidth = 640;  // karta ma ~340 px szerokości (x2 dla ekranów Retina)
        public const int MiniJpegQuality = 75;
        public const int JpegQuality = 80;
        private const long MaxPixels = 50_000_000; // ochrona przed "bombami dekompresyjnymi"

        public static bool IsSupportedExtension(string? extension) =>
            extension?.ToLowerInvariant() is ".jpg" or ".jpeg" or ".png" or ".gif";

        /// <summary>Wczytuje obraz; InvalidDataException, gdy to nie obraz lub jest za duży.</summary>
        public static Image Load(Stream input)
        {
            try
            {
                if (!input.CanSeek) throw new InvalidOperationException("Strumień musi umożliwiać cofanie.");

                var info = Image.Identify(input);
                if ((long)info.Width * info.Height > MaxPixels)
                    throw new InvalidDataException("Zdjęcie ma zbyt dużą rozdzielczość.");

                input.Position = 0;
                return Image.Load(input);
            }
            catch (Exception ex) when (ex is UnknownImageFormatException or InvalidImageContentException or NotSupportedException)
            {
                throw new InvalidDataException("Plik nie jest poprawnym obrazem.", ex);
            }
        }

        /// <summary>Obraca według EXIF, kasuje metadane i ogranicza wymiary (0 = bez ograniczenia).</summary>
        public static void Normalize(Image image, int maxWidth, int maxHeight = 0)
        {
            image.Mutate(x => x.AutoOrient());   // najpierw obrót, dopiero potem kasowanie EXIF
            image.Metadata.ExifProfile = null;
            image.Metadata.IptcProfile = null;
            image.Metadata.XmpProfile = null;

            if (maxWidth > 0 && image.Width > maxWidth)
                image.Mutate(x => x.Resize(maxWidth, 0));
            if (maxHeight > 0 && image.Height > maxHeight)
                image.Mutate(x => x.Resize(0, maxHeight));
        }

        /// <summary>Koduje obraz w formacie wynikającym z rozszerzenia (jpg/png/gif).</summary>
        public static void Encode(Image image, string extension, Stream output, int jpegQuality = JpegQuality)
        {
            switch (extension.ToLowerInvariant())
            {
                case ".jpg":
                case ".jpeg":
                    image.SaveAsJpeg(output, new JpegEncoder { Quality = jpegQuality });
                    break;
                case ".png":
                    image.SaveAsPng(output);
                    break;
                case ".gif":
                    image.SaveAsGif(output);
                    break;
                default:
                    throw new InvalidDataException("Nieobsługiwany format obrazu.");
            }
        }

        public static void Save(Image image, string path, int jpegQuality = JpegQuality)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            using var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
            Encode(image, Path.GetExtension(path), fs, jpegQuality);
        }

        /// <summary>Zapisuje oryginał (do MaxWidth) i miniaturę (bez powiększania).</summary>
        public static void SaveOriginalAndMini(Stream input, string originalPath, string miniPath, int miniHeight = MiniHeight)
        {
            using var image = Load(input);

            Normalize(image, MaxWidth);
            Save(image, originalPath);

            // miniatura mieści się w MiniWidth x miniHeight, bez powiększania
            double scale = Math.Min(1.0, Math.Min(MiniWidth / (double)image.Width,
                                                  miniHeight > 0 ? miniHeight / (double)image.Height : 1.0));
            if (scale < 1.0)
            {
                int w = Math.Max(1, (int)Math.Round(image.Width * scale));
                int h = Math.Max(1, (int)Math.Round(image.Height * scale));
                image.Mutate(x => x.Resize(w, h));
            }
            Save(image, miniPath, MiniJpegQuality);
        }

        public static MemoryStream ReadToMemory(IFormFile file)
        {
            var ms = new MemoryStream();
            using (var s = file.OpenReadStream()) s.CopyTo(ms);
            ms.Position = 0;
            return ms;
        }
    }
}
