using Hangfire.Dashboard;

namespace DoktarPlanning.Api.Middleware
{
    public class AllowAllDashboardAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            return true;
        }
    }

}
