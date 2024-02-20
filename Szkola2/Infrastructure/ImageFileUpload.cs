using System.Drawing;
using LazZiya.ImageResize;
using Microsoft.Extensions.Hosting;


namespace Szkola2.Infrastructure
{
    public class ImageFileUpload
    {
        private readonly IWebHostEnvironment hostingEnvironment;
        public ImageFileUpload(IWebHostEnvironment environment)
        {
            hostingEnvironment = environment;
        }
        private static bool FileTypeCheck(string extension)
        {
            return extension.ToLower() switch
            {
                ".jpg" or ".png" or ".gif" => true,
                _ => false,
            };
        }
        public FileSendResult SendFile(IFormFile picture, string destination, int width)
        {
            FileSendResult SendingFile = new();

            string extension = Path.GetExtension(picture.FileName);
            if (!FileTypeCheck(extension))
            {
                SendingFile.Name = Path.GetFileName(picture.FileName);
                SendingFile.Success = false;
                SendingFile.Error = "Niepoprawny typ pliku graficznego.";
                return SendingFile;

            }

            SendingFile.Name = Guid.NewGuid().ToString() + extension;

            var upload = Path.Combine(hostingEnvironment.WebRootPath, destination);
            var filePath = Path.Combine(upload, SendingFile.Name);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                picture.CopyTo(fileStream);
            }

            string path = Path.Combine(filePath);

            using (var imgFile = Image.FromFile(path))
            {
                var miniFile = imgFile.ScaleByWidth(width);
                upload = Path.Combine(hostingEnvironment.WebRootPath, destination + "\\mini");
                filePath = Path.Combine(upload, SendingFile.Name);
                miniFile.SaveAs(filePath);

            }

            SendingFile.Success = true;

            return SendingFile;

        }
        public FileSendResult SendPhoto(IFormFile photo, string destination, int width)
        {
            FileSendResult sendingFile = new();

            string extension = Path.GetExtension(photo.FileName);
            if (!FileTypeCheck(extension))
            {
                sendingFile.Name = Path.GetFileName(photo.FileName);
                sendingFile.Success = false;
                sendingFile.Error = "Niepoprawny typ pliku graficznego.";
                return sendingFile;
            }

            sendingFile.Name = Guid.NewGuid().ToString() + extension;

            var upload = Path.Combine(hostingEnvironment.WebRootPath, destination);
            var filePath = Path.Combine(upload, sendingFile.Name);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                photo.CopyTo(fileStream);
            }

            string path = Path.Combine(filePath);

            using (var imgFile = Image.FromFile(path))
            {
                var miniFile = imgFile.ScaleByWidth(width);
                upload = Path.Combine(hostingEnvironment.WebRootPath, destination + "\\zdjecia");
                filePath = Path.Combine(upload, sendingFile.Name);
                miniFile.SaveAs(filePath);
            }

            sendingFile.Success = true;

            return sendingFile;
        }
    

            public string UploadImage(string folderPath, IFormFile file)
        {
            folderPath += Guid.NewGuid().ToString() + "_" + file.FileName;
            string serverFolder = Path.Combine(hostingEnvironment.WebRootPath, folderPath);
            using (var fileStream = new FileStream(serverFolder, FileMode.Create))
            {
                file.CopyTo(fileStream);
            }
            //await file.CopyToAsync(new FileStream(serverFolder, FileMode.Create));
            return "\\" + folderPath;
        }
    }
}
