namespace Szkola2.Infrastructure
{
    public class DocumentFileUpload
    {
        private readonly IWebHostEnvironment hostEnvironment;
        public DocumentFileUpload(IWebHostEnvironment environment)
        {
            hostEnvironment = environment;
        }
        public FileSendResult DocumentSend (IFormFile document, string destination)
        {
            FileSendResult DocumentSending = new();

            string extension = Path.GetExtension(document.FileName);
            if (!FileTypeCheck(extension))
            {
                DocumentSending.Name = Path.GetFileName(document.FileName);
                DocumentSending.Success = false;
                DocumentSending.Error = "Niepoprawny typ pliku tekstowego.";
                return DocumentSending;
            }
            DocumentSending.Name = Guid.NewGuid().ToString() + extension;

            var upload = Path.Combine(hostEnvironment.WebRootPath, destination);
            var filePath = Path.Combine(upload, DocumentSending.Name);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                document.CopyTo(fileStream);
            }
            string path = Path.Combine(filePath);


            DocumentSending.Success = true;

            string pobierz = Path.Combine(destination,DocumentSending.Name); 
            DocumentSending.LinkURL = pobierz;
                return DocumentSending;
        }
        private static bool FileTypeCheck(string extension)
        {
            return extension.ToLower() switch
            {
                ".txt" or ".pdf" or ".docx" or ".doc" => true,
                _ => false,
            };
        }

    }
}
