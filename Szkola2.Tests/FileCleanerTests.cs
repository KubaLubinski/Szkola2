using Microsoft.Extensions.Logging.Abstractions;
using Szkola2.Infrastructure;
using Szkola2.Tests.Helpers;

namespace Szkola2.Tests
{
    public class FileCleanerTests : IDisposable
    {
        private readonly TestEnv _env = new();
        private readonly FileCleaner _cleaner;

        public FileCleanerTests() => _cleaner = new FileCleaner(_env, NullLogger<FileCleaner>.Instance);
        public void Dispose() => _env.Dispose();

        private string Create(params string[] relative)
        {
            var path = Path.Combine(new[] { _env.WebRootPath }.Concat(relative).ToArray());
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, "x");
            return path;
        }

        [Fact]
        public void DeleteMainGraphic_removes_original_and_mini()
        {
            var original = Create("img", "a.jpg");
            var mini = Create("img", "mini", "a.jpg");

            _cleaner.DeleteMainGraphic("a.jpg");

            Assert.False(File.Exists(original));
            Assert.False(File.Exists(mini));
        }

        [Fact]
        public void DeleteDocument_removes_file()
        {
            var doc = Create("documents", "regulamin.pdf");
            _cleaner.DeleteDocument("regulamin.pdf");
            Assert.False(File.Exists(doc));
        }

        [Fact]
        public void DeleteGalleryImage_accepts_both_slash_styles()
        {
            var a = Create("gallery", "a.jpg");
            _cleaner.DeleteGalleryImage("/gallery/a.jpg");
            Assert.False(File.Exists(a));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Empty_names_are_ignored(string? name)
        {
            var exception = Record.Exception(() =>
            {
                _cleaner.DeleteMainGraphic(name);
                _cleaner.DeleteDocument(name);
                _cleaner.DeleteGalleryImage(name);
            });
            Assert.Null(exception);
        }

        [Fact]
        public void Missing_file_does_not_throw()
            => Assert.Null(Record.Exception(() => _cleaner.DeleteMainGraphic("nie-ma.jpg")));

        [Fact]
        public void Cannot_delete_files_outside_wwwroot()
        {
            var secret = Path.Combine(_env.BaseDir, "secret.txt");
            File.WriteAllText(secret, "tajne");

            // wwwroot/img/../../secret.txt wskazuje poza wwwroot
            _cleaner.DeleteMainGraphic("../../secret.txt");

            Assert.True(File.Exists(secret));
        }
    }
}
