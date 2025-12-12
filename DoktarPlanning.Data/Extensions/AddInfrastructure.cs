using DoktarPlanning.Data.Contexts;
using DoktarPlanning.Data.Interceptors;
using DoktarPlanning.Data.Repositories;
using DoktarPlanning.Domain.Interfaces;
using DoktarPlanning.Infrastructure.Repositories.Interfaces;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DoktarPlanning.Data.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddData(this IServiceCollection services, IConfiguration configuration)
        {
            var conn = configuration.GetConnectionString("DefaultConnection")
                       ?? throw new InvalidOperationException("DefaultConnection not configured.");

            services.AddDbContext<AppDbContext>((provider, options) =>
            {
                options.UseSqlServer(conn, sql =>
                {
                    sql.EnableRetryOnFailure();
                });

                var interceptor = provider.GetRequiredService<LoggingSaveChangesInterceptor>();
                options.AddInterceptors(interceptor);
            });

            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            services.AddScoped<ITaskRepository, TaskRepository>();
            services.AddScoped<IRecurrenceRuleRepository, RecurrenceRuleRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ISubTaskRepository, SubTaskRepository>();

            return services;
        }
    }
}