using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.PixelFormats;
using Szkola2.Infrastructure;
using Szkola2.Tests.Helpers;

namespace Szkola2.Tests
{
    public class ImageProcessingTests : IDisposable
    {
        private readonly TestEnv _env = new();
        public void Dispose() => _env.Dispose();

        [Theory]
        [InlineData(".jpg", true)]
        [InlineData(".JPEG", true)]
        [InlineData(".png", true)]
        [InlineData(".gif", true)]
        [InlineData(".svg", false)]
        [InlineData(".exe", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void IsSupportedExtension_checks_whitelist(string? extension, bool expected)
            => Assert.Equal(expected, ImageProcessing.IsSupportedExtension(extension));

        [Fact]
        public void Load_accepts_valid_image()
        {
            using var image = ImageProcessing.Load(Files.Png(40, 30));
            Assert.Equal(40, image.Width);
            Assert.Equal(30, image.Height);
        }

        [Fact]
        public void Load_rejects_non_image_data()
        {
            var fake = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("<html>to nie jest obraz</html>"));
            Assert.Throws<InvalidDataException>(() => ImageProcessing.Load(fake));
        }

        [Fact]
        public void Normalize_shrinks_wide_image_keeping_proportions()
        {
            using var image = new Image<Rgba32>(3000, 1000);
            ImageProcessing.Normalize(image, ImageProcessing.MaxWidth);

            Assert.Equal(1920, image.Width);
            Assert.Equal(640, image.Height);
        }

        [Fact]
        public void Normalize_does_not_enlarge_small_image()
        {
            using var image = new Image<Rgba32>(800, 600);
            ImageProcessing.Normalize(image, ImageProcessing.MaxWidth);

            Assert.Equal(800, image.Width);
            Assert.Equal(600, image.Height);
        }

        [Fact]
        public void Normalize_removes_exif_metadata()
        {
            using var image = new Image<Rgba32>(100, 100);
            var exif = new ExifProfile();
            exif.SetValue(ExifTag.Software, "aparat-testowy");
            image.Metadata.ExifProfile = exif;

            ImageProcessing.Normalize(image, ImageProcessing.MaxWidth);

            Assert.Null(image.Metadata.ExifProfile);
        }

        [Fact]
        public void SaveOriginalAndMini_creates_resized_original_and_small_thumbnail()
        {
            var original = Path.Combine(_env.WebRootPath, "img", "foto.jpg");
            var mini = Path.Combine(_env.WebRootPath, "img", "mini", "foto.jpg");

            ImageProcessing.SaveOriginalAndMini(Files.Png(4000, 3000), original, mini);

            var originalInfo = Image.Identify(original);
            var miniInfo = Image.Identify(mini);
            Assert.Equal(1920, originalInfo.Width);
            Assert.Equal(1440, originalInfo.Height);
            Assert.True(miniInfo.Width <= ImageProcessing.MiniWidth);
            Assert.True(miniInfo.Height <= ImageProcessing.MiniHeight);
            Assert.True(new FileInfo(mini).Length < new FileInfo(original).Length);
        }

        [Fact]
        public void SaveOriginalAndMini_does_not_enlarge_small_images()
        {
            var original = Path.Combine(_env.WebRootPath, "o.jpg");
            var mini = Path.Combine(_env.WebRootPath, "m.jpg");

            ImageProcessing.SaveOriginalAndMini(Files.Png(200, 100), original, mini);

            var miniInfo = Image.Identify(mini);
            Assert.Equal(200, miniInfo.Width);
            Assert.Equal(100, miniInfo.Height);
        }
    }
}
