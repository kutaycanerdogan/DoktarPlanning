using DoktarPlanning.Application.Services;
using DoktarPlanning.Infrastructure.Interfaces;

using Microsoft.Extensions.DependencyInjection;

namespace DoktarPlanning.Application.Extensions
{

    public static class ApplicationServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<ITaskService, TaskService>();
            services.AddScoped<IRecurrenceService, RecurrenceService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ISubTaskService, SubTaskService>();
            services.AddScoped<IReminderService, ReminderService>();
            services.AddScoped<IRecurrenceJobRunner, RecurrenceJobRunner>();

            // Infrastructure services
            services.AddScoped<IEmailSender, SmtpEmailSender>();
            services.AddHttpClient<IWebhookSender, HttpWebhookSender>();
            services.AddScoped<IBackgroundJobService, HangfireBackgroundJobService>();

            return services;
        }
    }
}
