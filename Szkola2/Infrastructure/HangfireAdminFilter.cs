using Hangfire.Dashboard;

namespace Szkola2.Infrastructure
{
    /// <summary>Panel Hangfire tylko dla admina (nie dla konta demo).</summary>
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
