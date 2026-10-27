using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;

namespace Szkola2.Tests.Helpers
{
    /// <summary>Udaje środowisko aplikacji z wwwroot w katalogu tymczasowym (sprzątanym po teście).</summary>
    public sealed class TestEnv : IWebHostEnvironment, IDisposable
    {
        public string BaseDir { get; }
        public string WebRootPath { get; set; }
        public string ContentRootPath { get; set; }
        public string EnvironmentName { get; set; } = "Test";
        public string ApplicationName { get; set; } = "Szkola2.Tests";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();

        public TestEnv()
        {
            BaseDir = Path.Combine(Path.GetTempPath(), "szkola2-tests-" + Guid.NewGuid().ToString("N"));
            WebRootPath = Path.Combine(BaseDir, "wwwroot");
            ContentRootPath = BaseDir;
            Directory.CreateDirectory(WebRootPath);
        }

        public void Dispose()
        {
            try { Directory.Delete(BaseDir, recursive: true); } catch (IOException) { }
        }
    }
}
