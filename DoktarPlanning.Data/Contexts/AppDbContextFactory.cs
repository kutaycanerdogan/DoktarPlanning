using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace DoktarPlanning.Data.Contexts
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var basePath = Directory.GetCurrentDirectory();

            var apiPath = Path.Combine(basePath, "..", "DoktarPlanning.Api");
            if (Directory.Exists(apiPath))
                basePath = apiPath;

            var builder = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .AddJsonFile($"appsettings.Development.json", optional: true, reloadOnChange: false)
                .AddJsonFile($"appsettings.Docker.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables();

            var config = builder.Build();

            var conn = config.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(conn))
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found. appsettings.json veya environment variables kontrol edin.");
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            if (!optionsBuilder.IsConfigured && Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Testing")
            {
                optionsBuilder.UseSqlServer(conn, sql => sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));
            }
            return new AppDbContext(optionsBuilder.Options);
        }
    }
}