using Ganss.Xss;

namespace Szkola2.Infrastructure
{
    /// <summary>
    /// Czyści HTML z edytora (CKEditor) przed zapisem do bazy: usuwa skrypty, zdarzenia onclick/onerror,
    /// adresy javascript: itp. Widoki wypisują treść przez Html.Raw, więc bez tego każdy użytkownik
    /// z prawem dodawania tekstów mógłby wstrzyknąć kod wykonywany u wszystkich odwiedzających.
    /// Wymaga pakietu NuGet "HtmlSanitizer".
    /// </summary>
    public static class HtmlCleaner
    {
        public static string? Clean(string? html)
        {
            if (string.IsNullOrWhiteSpace(html)) return html;

            // nowa instancja przy każdym wywołaniu - zapisy są rzadkie, a unikamy współdzielenia stanu
            var sanitizer = new HtmlSanitizer();
            sanitizer.AllowedAttributes.Add("class");
            sanitizer.AllowedAttributes.Add("target");
            sanitizer.AllowedAttributes.Add("rel");

            // linki otwierane w nowej karcie nie mogą dostać dostępu do window.opener (tabnabbing)
            sanitizer.PostProcessNode += (_, e) =>
            {
                if (e.Node is AngleSharp.Dom.IElement element
                    && element.LocalName == "a"
                    && element.HasAttribute("target"))
                {
                    element.SetAttribute("rel", "noopener noreferrer");
                }
            };

            return sanitizer.Sanitize(html);
        }
    }
}
