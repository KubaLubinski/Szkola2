using System.Net;
using System.Text.RegularExpressions;

namespace Szkola2.Infrastructure
{
    /// <summary>Podgląd treści HTML jako zwykły tekst. Wypisywać przez @, nie Html.Raw.</summary>
    public static partial class TextPreview
    {
        public static string FromHtml(string? html, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(html)) return string.Empty;

            var withSpaces = TagRegex().Replace(html, " ");
            var plain = WebUtility.HtmlDecode(withSpaces);
            plain = WhitespaceRegex().Replace(plain, " ").Trim();

            if (plain.Length <= maxLength) return plain;

            var cut = plain[..maxLength];
            var lastSpace = cut.LastIndexOf(' ');
            if (lastSpace > maxLength / 2) cut = cut[..lastSpace];
            return cut.TrimEnd() + "...";
        }

        [GeneratedRegex("<[^>]*>")]
        private static partial Regex TagRegex();

        [GeneratedRegex(@"\s+")]
        private static partial Regex WhitespaceRegex();
    }
}
