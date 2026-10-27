using Ganss.Xss;

namespace Szkola2.Infrastructure
{
    /// <summary>Czyści HTML z edytora przed zapisem (skrypty, onclick, javascript:).</summary>
    public static class HtmlCleaner
    {
        public static string? Clean(string? html)
        {
            if (string.IsNullOrWhiteSpace(html)) return html;

            var sanitizer = new HtmlSanitizer();
            sanitizer.AllowedAttributes.Add("class");
            sanitizer.AllowedAttributes.Add("target");
            sanitizer.AllowedAttributes.Add("rel");

            // target=_blank bez dostępu do window.opener
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
