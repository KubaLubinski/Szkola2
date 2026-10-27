using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Szkola2.Models;

namespace Szkola2.Infrastructure
{
    /// <summary>Nazwy claimów konta demo.</summary>
    public static class DemoClaims
    {
        public const string IsDemo = "szkola:demo";
        public const string Expires = "szkola:demo_exp";

        public static string? GetDemoUserId(ClaimsPrincipal? user)
        {
            if (user?.Identity?.IsAuthenticated != true || !user.HasClaim(IsDemo, "1"))
                return null;
            return user.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        public static DateTime? GetExpiresUtc(ClaimsPrincipal? user)
        {
            var raw = user?.FindFirstValue(Expires);
            return long.TryParse(raw, out var seconds)
                ? DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime
                : null;
        }
    }

    public static class DemoMessages
    {
        public const string Key = "DemoMessage";
        public const string ReadOnly = "Konto testowe nie może zapisywać zmian w treściach szkoły. Możesz dodawać, edytować i usuwać tylko własne wpisy.";
        public const string ViewOnly = "To jest treść szkoły – w koncie testowym możesz ją tylko obejrzeć. Zapis zmian jest wyłączony.";
    }

    public static class DemoLimits
    {
        public const int SessionHours = 2;
        public const int MaxActiveAccounts = 50;
        public const int MaxStartsPerIpPerHour = 5;

        public const int MaxTexts = 5;
        public const int MaxDocuments = 5;
        public const int MaxEvents = 5;
        public const int MaxGalleryFiles = 5;
        public const long MaxFileBytes = 5 * 1024 * 1024;

        public static readonly string[] DocumentExtensions = { ".pdf", ".docx", ".doc", ".txt" };

        /// <summary>Zwraca błąd albo null, gdy pliki mieszczą się w limitach.</summary>
        public static string? CheckFiles(IFormFile? main, IFormFileCollection? gallery)
        {
            if (main != null && main.Length > MaxFileBytes)
                return $"Konto testowe: maksymalny rozmiar pliku to {MaxFileBytes / 1024 / 1024} MB.";
            if (gallery == null) return null;
            var files = gallery.Where(f => f.Length > 0).ToList();
            if (files.Count > MaxGalleryFiles)
                return $"Konto testowe: maksymalnie {MaxGalleryFiles} zdjęć w galerii.";
            if (files.Any(f => f.Length > MaxFileBytes))
                return $"Konto testowe: maksymalny rozmiar pliku to {MaxFileBytes / 1024 / 1024} MB.";
            return null;
        }

        public static string MaskEmail(string? email)
        {
            if (string.IsNullOrEmpty(email)) return "";
            var at = email.IndexOf('@');
            return at <= 0 ? "***" : email[0] + "***" + email[at..];
        }
    }

    /// <summary>Próba zapisu cudzych danych przez konto demo.</summary>
    public class DemoReadOnlyException : InvalidOperationException
    {
        public DemoReadOnlyException() : base(DemoMessages.ReadOnly) { }
    }

    public interface IDemoContext
    {
        bool IsDemo { get; }
        string? UserId { get; }
        DateTime? ExpiresUtc { get; }
        /// <summary>Zwykły użytkownik: zawsze true. Konto demo: tylko własne rekordy.</summary>
        bool CanModify(IDemoOwned entity);
    }

    public class DemoContext : IDemoContext
    {
        private readonly IHttpContextAccessor _accessor;
        public DemoContext(IHttpContextAccessor accessor) => _accessor = accessor;

        private ClaimsPrincipal? User => _accessor.HttpContext?.User;
        public string? UserId => DemoClaims.GetDemoUserId(User);
        public bool IsDemo => UserId != null;
        public DateTime? ExpiresUtc => IsDemo ? DemoClaims.GetExpiresUtc(User) : null;

        public bool CanModify(IDemoOwned entity)
            => !IsDemo || (entity.DemoOwnerId != null && entity.DemoOwnerId == UserId);
    }

    /// <summary>Komunikat i powrót na poprzednią stronę zamiast błędu dla konta demo.</summary>
    public class DemoExceptionFilter : IExceptionFilter
    {
        private readonly ITempDataDictionaryFactory _tempData;
        public DemoExceptionFilter(ITempDataDictionaryFactory tempData) => _tempData = tempData;

        public void OnException(ExceptionContext context)
        {
            if (context.Exception is not DemoReadOnlyException ex) return;

            _tempData.GetTempData(context.HttpContext)[DemoMessages.Key] = ex.Message;

            var target = "/";
            var referer = context.HttpContext.Request.Headers.Referer.ToString();
            if (Uri.TryCreate(referer, UriKind.Absolute, out var uri)
                && string.Equals(uri.Host, context.HttpContext.Request.Host.Host, StringComparison.OrdinalIgnoreCase))
            {
                target = uri.PathAndQuery;
            }
            context.Result = new RedirectResult(target);
            context.ExceptionHandled = true;
        }
    }
}
