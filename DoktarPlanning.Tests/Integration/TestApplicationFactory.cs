using BCrypt.Net;

using DoktarPlanning.Data.Contexts;
using DoktarPlanning.Domain.Entities;
using DoktarPlanning.Infrastructure.Interfaces;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Moq;

namespace DoktarPlanning.Tests.Integration
{
    public class TestApplicationFactory : WebApplicationFactory<Program>
    {
        private readonly SqliteConnection _connection;

        public TestApplicationFactory()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
            });
            builder.ConfigureServices(services =>
            {
                // Remove real DB
                var contextDescriptors = services.Remove(
                    services.SingleOrDefault(d => d.ServiceType == typeof(IDbContextOptionsConfiguration<AppDbContext>))
                );

                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseSqlite(_connection);
                });

                var sp = services.BuildServiceProvider();
                using (var scope = sp.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    db.Database.EnsureCreated();

                    // Seed test user
                    var testUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");

                    if (!db.Users.Any(u => u.Id == testUserId))
                    {
                        db.Users.Add(new User
                        {
                            Id = testUserId,
                            Email = "kutaycan01@gmail.com",
                            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"),
                        });

                        db.SaveChanges();
                    }
                }

                services.AddSingleton(Mock.Of<IBackgroundJobService>());

                services.AddSingleton(Mock.Of<IEmailSender>());

                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "Test";
                    options.DefaultChallengeScheme = "Test";
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });
            });

            builder.UseEnvironment("Testing");
        }
    }
}