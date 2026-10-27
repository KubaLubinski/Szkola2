using Szkola2.Infrastructure;

namespace Szkola2.Tests
{
    public class HtmlCleanerTests
    {
        [Fact]
        public void Removes_script_tags()
        {
            var result = HtmlCleaner.Clean("<p>Cześć</p><script>alert(1)</script>");
            Assert.Contains("<p>Cześć</p>", result);
            Assert.DoesNotContain("script", result, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("alert", result);
        }

        [Fact]
        public void Removes_event_handlers()
        {
            var result = HtmlCleaner.Clean("<img src=\"x.jpg\" onerror=\"alert(1)\">");
            Assert.DoesNotContain("onerror", result, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Removes_javascript_urls()
        {
            var result = HtmlCleaner.Clean("<a href=\"javascript:alert(1)\">kliknij</a>");
            Assert.DoesNotContain("javascript:", result, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Keeps_basic_formatting_and_links()
        {
            var result = HtmlCleaner.Clean("<p><strong>Ważne</strong> <a href=\"https://example.com\">link</a></p>");
            Assert.Contains("<strong>Ważne</strong>", result);
            Assert.Contains("href=\"https://example.com\"", result);
        }

        [Fact]
        public void Links_opening_in_new_tab_get_noopener()
        {
            var result = HtmlCleaner.Clean("<a href=\"https://example.com\" target=\"_blank\">link</a>");
            Assert.Contains("rel=\"noopener noreferrer\"", result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Null_or_empty_is_returned_unchanged(string? html)
            => Assert.Equal(html, HtmlCleaner.Clean(html));
    }
}
