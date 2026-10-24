using Hangfire.Dashboard;

namespace Szkola2.Infrastructure
{
    /// <summary>
    /// Panel Hangfire tylko dla zalogowanego administratora (nie dla konta testowego, które też ma rolę admin).
    /// Domyślny filtr Hangfire wpuszcza każde żądanie lokalne - za reverse proxy na tym samym serwerze to ryzykowne.
    /// </summary>
    public class HangfireAdminFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var user = context.GetHttpContext().User;
            return user.Identity?.IsAuthenticated == true
                   && user.IsInRole("admin")
                   && DemoClaims.GetDemoUserId(user) == null;
        }
    }
}
