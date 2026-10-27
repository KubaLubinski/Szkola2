using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Szkola2.Infrastructure;
using Szkola2.Models;
using Szkola2.Tests.Helpers;

namespace Szkola2.Tests
{
    public class DemoSupportTests
    {
        private static ClaimsPrincipal Principal(params Claim[] claims)
            => new(new ClaimsIdentity(claims, authenticationType: "test"));

        private static ClaimsPrincipal DemoUser(string id = "demo-1")
            => Principal(new Claim(ClaimTypes.NameIdentifier, id), new Claim(DemoClaims.IsDemo, "1"));

        private static DemoContext ContextFor(ClaimsPrincipal user)
            => new(new HttpContextAccessor { HttpContext = new DefaultHttpContext { User = user } });

        private sealed class Owned : IDemoOwned
        {
            public string? DemoOwnerId { get; set; }
        }

        // --- DemoClaims ---

        [Fact]
        public void Demo_claim_gives_user_id()
            => Assert.Equal("demo-1", DemoClaims.GetDemoUserId(DemoUser("demo-1")));

        [Fact]
        public void Regular_user_is_not_demo()
            => Assert.Null(DemoClaims.GetDemoUserId(Principal(new Claim(ClaimTypes.NameIdentifier, "u1"))));

        [Fact]
        public void Anonymous_user_is_not_demo()
            => Assert.Null(DemoClaims.GetDemoUserId(new ClaimsPrincipal(new ClaimsIdentity())));

        [Fact]
        public void Expiry_is_read_from_unix_seconds()
        {
            var expires = new DateTime(2030, 1, 2, 3, 4, 5, DateTimeKind.Utc);
            var user = Principal(new Claim(DemoClaims.Expires, new DateTimeOffset(expires).ToUnixTimeSeconds().ToString()));
            Assert.Equal(expires, DemoClaims.GetExpiresUtc(user));
        }

        [Fact]
        public void Invalid_expiry_gives_null()
            => Assert.Null(DemoClaims.GetExpiresUtc(Principal(new Claim(DemoClaims.Expires, "nie-liczba"))));

        // --- DemoContext.CanModify ---

        [Fact]
        public void Regular_user_can_modify_everything()
        {
            var ctx = ContextFor(Principal(new Claim(ClaimTypes.NameIdentifier, "u1")));
            Assert.False(ctx.IsDemo);
            Assert.True(ctx.CanModify(new Owned { DemoOwnerId = null }));
            Assert.True(ctx.CanModify(new Owned { DemoOwnerId = "ktos-inny" }));
        }

        [Fact]
        public void Demo_user_can_modify_only_own_records()
        {
            var ctx = ContextFor(DemoUser("demo-1"));
            Assert.True(ctx.IsDemo);
            Assert.True(ctx.CanModify(new Owned { DemoOwnerId = "demo-1" }));
            Assert.False(ctx.CanModify(new Owned { DemoOwnerId = "demo-2" }));
        }

        [Fact]
        public void Demo_user_cannot_modify_school_content()
        {
            // rekordy szkoły nie mają właściciela (DemoOwnerId == null)
            var ctx = ContextFor(DemoUser());
            Assert.False(ctx.CanModify(new Owned { DemoOwnerId = null }));
        }

        // --- DemoLimits ---

        [Theory]
        [InlineData("jan.kowalski@szkola.pl", "j***@szkola.pl")]
        [InlineData("a@b.pl", "a***@b.pl")]
        [InlineData("bez-malpy", "***")]
        [InlineData("@x.pl", "***")]
        [InlineData("", "")]
        [InlineData(null, "")]
        public void MaskEmail_hides_local_part(string? email, string expected)
            => Assert.Equal(expected, DemoLimits.MaskEmail(email));

        [Fact]
        public void CheckFiles_accepts_files_within_limits()
        {
            var gallery = new FormFileCollection();
            for (int i = 0; i < DemoLimits.MaxGalleryFiles; i++) gallery.Add(Files.OfSize(1000));

            Assert.Null(DemoLimits.CheckFiles(Files.OfSize(1000), gallery));
        }

        [Fact]
        public void CheckFiles_rejects_too_large_main_file()
            => Assert.NotNull(DemoLimits.CheckFiles(Files.OfSize(DemoLimits.MaxFileBytes + 1), null));

        [Fact]
        public void CheckFiles_rejects_too_many_gallery_files()
        {
            var gallery = new FormFileCollection();
            for (int i = 0; i < DemoLimits.MaxGalleryFiles + 1; i++) gallery.Add(Files.OfSize(1000));

            Assert.NotNull(DemoLimits.CheckFiles(null, gallery));
        }

        [Fact]
        public void CheckFiles_rejects_too_large_gallery_file()
        {
            var gallery = new FormFileCollection { Files.OfSize(DemoLimits.MaxFileBytes + 1) };
            Assert.NotNull(DemoLimits.CheckFiles(null, gallery));
        }
    }
}
