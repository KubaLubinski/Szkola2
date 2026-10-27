namespace Szkola2.Infrastructure
{
    public class DocumentFileUpload
    {
        private readonly IWebHostEnvironment hostEnvironment;

        public DocumentFileUpload(IWebHostEnvironment environment)
        {
            hostEnvironment = environment;
        }

        public FileSendResult DocumentSend(IFormFile document, string destination)
        {
            FileSendResult result = new();

            string extension = Path.GetExtension(document.FileName).ToLowerInvariant();
            if (!FileTypeCheck(extension) || !ContentMatchesExtension(document, extension))
            {
                result.Name = Path.GetFileName(document.FileName);
                result.Success = false;
                result.Error = "Niepoprawny typ pliku tekstowego.";
                return result;
            }

            result.Name = Guid.NewGuid().ToString() + extension;

            var upload = Path.Combine(hostEnvironment.WebRootPath, destination);
            Directory.CreateDirectory(upload);
            var filePath = Path.Combine(upload, result.Name);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                document.CopyTo(fileStream);
            }

            result.Success = true;
            result.LinkURL = Path.Combine(destination, result.Name);
            return result;
        }

        private static bool FileTypeCheck(string extension)
        {
            return extension switch
            {
                ".txt" or ".pdf" or ".docx" or ".doc" => true,
                _ => false,
            };
        }

        /// <summary>Sprawdza początek pliku (PDF: %PDF, DOCX: PK, DOC: nagłówek OLE, TXT: bez bajtów zerowych).</summary>
        private static bool ContentMatchesExtension(IFormFile file, string extension)
        {
            byte[] head = new byte[8192];
            int read;
            using (var stream = file.OpenReadStream())
            {
                read = stream.Read(head, 0, head.Length);
            }
            if (read == 0) return false;

            return extension switch
            {
                ".pdf" => read >= 4 && head[0] == 0x25 && head[1] == 0x50 && head[2] == 0x44 && head[3] == 0x46,
                ".docx" => read >= 4 && head[0] == 0x50 && head[1] == 0x4B,
                ".doc" => read >= 4 && head[0] == 0xD0 && head[1] == 0xCF && head[2] == 0x11 && head[3] == 0xE0,
                ".txt" => !head.AsSpan(0, read).Contains((byte)0),
                _ => false,
            };
        }
    }
}
