using System.Text;
using Szkola2.Infrastructure;
using Szkola2.Tests.Helpers;

namespace Szkola2.Tests
{
    public class DocumentFileUploadTests : IDisposable
    {
        private readonly TestEnv _env = new();
        private readonly DocumentFileUpload _upload;

        public DocumentFileUploadTests() => _upload = new DocumentFileUpload(_env);

        public void Dispose() => _env.Dispose();

        private static readonly byte[] PdfBytes = Encoding.ASCII.GetBytes("%PDF-1.4\n1 0 obj\n<<>>\nendobj\n");
        private static readonly byte[] ZipBytes = { 0x50, 0x4B, 0x03, 0x04, 0x00, 0x00 };
        private static readonly byte[] OleBytes = { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 };

        [Theory]
        [InlineData("regulamin.pdf")]
        [InlineData("regulamin.docx")]
        [InlineData("regulamin.doc")]
        [InlineData("regulamin.txt")]
        public void Accepts_matching_content(string fileName)
        {
            var bytes = Path.GetExtension(fileName) switch
            {
                ".pdf" => PdfBytes,
                ".docx" => ZipBytes,
                ".doc" => OleBytes,
                _ => Encoding.UTF8.GetBytes("zwykły tekst")
            };

            var result = _upload.DocumentSend(Files.Form(bytes, fileName), "documents");

            Assert.True(result.Success, result.Error);
            Assert.True(File.Exists(Path.Combine(_env.WebRootPath, "documents", result.Name!)));
        }

        [Fact]
        public void Saved_name_is_random_not_the_original()
        {
            var result = _upload.DocumentSend(Files.Form(PdfBytes, "umowa.pdf"), "documents");

            Assert.True(result.Success);
            Assert.NotEqual("umowa.pdf", result.Name);
            Assert.EndsWith(".pdf", result.Name);
        }

        [Fact]
        public void Rejects_html_disguised_as_pdf()
        {
            var result = _upload.DocumentSend(Files.Form("<html><script>alert(1)</script></html>", "faktura.pdf"), "documents");

            Assert.False(result.Success);
            Assert.NotNull(result.Error);
            Assert.False(Directory.Exists(Path.Combine(_env.WebRootPath, "documents")));
        }

        [Theory]
        [InlineData("program.exe")]
        [InlineData("strona.html")]
        [InlineData("skrypt.js")]
        [InlineData("bezrozszerzenia")]
        public void Rejects_disallowed_extensions(string fileName)
            => Assert.False(_upload.DocumentSend(Files.Form(PdfBytes, fileName), "documents").Success);

        [Fact]
        public void Rejects_empty_file()
            => Assert.False(_upload.DocumentSend(Files.Form(Array.Empty<byte>(), "pusty.pdf"), "documents").Success);

        [Fact]
        public void Rejects_text_file_with_binary_content()
            => Assert.False(_upload.DocumentSend(Files.Form(new byte[] { 0x41, 0x00, 0x42 }, "dziwny.txt"), "documents").Success);

        [Fact]
        public void Path_in_file_name_cannot_escape_destination()
        {
            var result = _upload.DocumentSend(Files.Form(PdfBytes, "../../evil.pdf"), "documents");

            Assert.True(result.Success);
            Assert.DoesNotContain("..", result.Name);
            Assert.DoesNotContain("/", result.Name);
            Assert.False(File.Exists(Path.Combine(_env.BaseDir, "evil.pdf")));
        }
    }
}
