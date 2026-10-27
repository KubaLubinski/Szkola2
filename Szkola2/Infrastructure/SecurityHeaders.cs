namespace Szkola2.Infrastructure
{
    /// <summary>Nagłówki bezpieczeństwa; CSP dopuszcza inline, bo strona go używa.</summary>
    public static class SecurityHeaders
    {
        private const string ContentSecurityPolicy =
            "default-src 'self'; " +
            "script-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net https://cdnjs.cloudflare.com https://unpkg.com https://cdn.datatables.net; " +
            "style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net https://cdnjs.cloudflare.com https://cdn.datatables.net; " +
            "font-src 'self' https://cdn.jsdelivr.net; " +
            "img-src 'self' data: https:; " +
            "frame-src https://www.google.com; " +
            "connect-src 'self'; " +
            "object-src 'none'; base-uri 'self'; form-action 'self'; frame-ancestors 'none'";

        public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
        {
            return app.Use(async (context, next) =>
            {
                var headers = context.Response.Headers;
                headers["X-Content-Type-Options"] = "nosniff";
                headers["X-Frame-Options"] = "DENY";
                headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
                headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=(), payment=()";
                headers["Content-Security-Policy"] = ContentSecurityPolicy;
                await next();
            });
        }
    }
}
