using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Szkola2.Infrastructure;

namespace Szkola2.Tests
{
    public class SecurityHeadersTests
    {
        private static async Task<IHeaderDictionary> RunAsync()
        {
            var builder = new ApplicationBuilder(new ServiceCollection().BuildServiceProvider());
            builder.UseSecurityHeaders();
            builder.Run(_ => Task.CompletedTask);

            var context = new DefaultHttpContext();
            await builder.Build()(context);
            return context.Response.Headers;
        }

        [Fact]
        public async Task Sets_basic_protection_headers()
        {
            var headers = await RunAsync();

            Assert.Equal("nosniff", headers["X-Content-Type-Options"]);
            Assert.Equal("DENY", headers["X-Frame-Options"]);
            Assert.Equal("strict-origin-when-cross-origin", headers["Referrer-Policy"]);
            Assert.False(string.IsNullOrEmpty(headers["Permissions-Policy"]));
        }

        [Fact]
        public async Task Content_security_policy_blocks_plugins_and_framing()
        {
            var csp = (await RunAsync())["Content-Security-Policy"].ToString();

            Assert.Contains("default-src 'self'", csp);
            Assert.Contains("object-src 'none'", csp);
            Assert.Contains("frame-ancestors 'none'", csp);
        }
    }
}
