using System.Text;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Szkola2.Tests.Helpers
{
    internal static class Files
    {
        public static IFormFile Form(byte[] content, string fileName)
            => new FormFile(new MemoryStream(content), 0, content.Length, "file", fileName);

        public static IFormFile Form(string text, string fileName)
            => Form(Encoding.UTF8.GetBytes(text), fileName);

        /// <summary>Plik o zadeklarowanym rozmiarze bez alokowania pamięci (do testów limitów).</summary>
        public static IFormFile OfSize(long length, string fileName = "a.jpg")
            => new FormFile(Stream.Null, 0, length, "file", fileName);

        /// <summary>Poprawny obraz PNG o podanych wymiarach, wygenerowany w pamięci.</summary>
        public static MemoryStream Png(int width, int height)
        {
            using var image = new Image<Rgba32>(width, height, new Rgba32(30, 120, 200));
            var ms = new MemoryStream();
            image.SaveAsPng(ms);
            ms.Position = 0;
            return ms;
        }
    }
}
