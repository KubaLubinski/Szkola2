using Szkola2.Infrastructure;

namespace Szkola2.Tests
{
    public class TextPreviewTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Empty_input_gives_empty_string(string? html)
            => Assert.Equal(string.Empty, TextPreview.FromHtml(html, 100));

        [Fact]
        public void Strips_tags_and_collapses_whitespace()
            => Assert.Equal("Ala ma kota", TextPreview.FromHtml("<p>Ala  ma <b>kota</b></p>\n", 100));

        [Fact]
        public void Decodes_html_entities()
            => Assert.Equal("Tom & Jerry", TextPreview.FromHtml("<p>Tom &amp; Jerry</p>", 100));

        [Fact]
        public void Short_text_is_not_truncated()
            => Assert.Equal("krótki tekst", TextPreview.FromHtml("krótki tekst", 50));

        [Fact]
        public void Long_text_is_cut_at_word_boundary()
            => Assert.Equal("aaaa bbbb...", TextPreview.FromHtml("aaaa bbbb cccc dddd", 12));

        [Fact]
        public void Text_without_spaces_is_cut_at_limit()
            => Assert.Equal("abcde...", TextPreview.FromHtml("abcdefghijklmnop", 5));

        [Fact]
        public void Never_cuts_a_tag_in_half()
        {
            // dawny błąd: Substring na gotowym HTML mógł przeciąć znacznik i "połknąć" resztę strony
            var result = TextPreview.FromHtml("<p>Zaproszenie na <a href=\"https://example.com/bardzo/dlugi/adres\">uroczystość</a></p>", 15);
            Assert.DoesNotContain("<", result);
            Assert.DoesNotContain("href", result);
        }
    }
}
