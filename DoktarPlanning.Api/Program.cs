using Azure.Identity;

using DoktarPlanning.Api.Middleware;
using DoktarPlanning.Application.Extensions;
using DoktarPlanning.Application.Mapping;
using DoktarPlanning.Data.Contexts;
using DoktarPlanning.Data.Extensions;
using DoktarPlanning.Data.Interceptors;

using Hangfire;
using Hangfire.Dashboard;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

using Serilog;

using StackExchange.Redis;

using System.Text;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;
var env = builder.Environment;

// KeyVault + Kestrel (production only)
if (!env.IsDevelopment())
{
    //var keyVaultUri = config["KeyVault:VaultUri"];
    //if (!string.IsNullOrWhiteSpace(keyVaultUri))
    //{
    //    builder.Configuration.AddAzureKeyVault(new Uri(keyVaultUri), new DefaultAzureCredential());
    //}

    //builder.WebHost.ConfigureKestrel(options =>
    //{
    //    var certPath = builder.Configuration["Kestrel:Endpoints:Https:Certificate:Path"];
    //    var certPassword = builder.Configuration["Kestrel:Endpoints:Https:Certificate:Password"];
    //    if (!string.IsNullOrWhiteSpace(certPath) && File.Exists(certPath))
    //    {
    //        options.ListenAnyIP(443, lo => lo.UseHttps(certPath, certPassword));
    //    }
    //});
}

// Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(config)
    .Enrich.FromLogContext()
    .CreateLogger();
builder.Host.UseSerilog();

// DI registrations
builder.Services.AddScoped<LoggingSaveChangesInterceptor>();
builder.Services.AddData(config);
builder.Services.AddHttpClient();
builder.Services.AddApplicationServices();
builder.Services.AddAutoMapper(cfg => cfg.AddProfile(new MappingProfile()));

// Redis
var redisConfig = config["Redis:Configuration"];
if (!string.IsNullOrWhiteSpace(redisConfig))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConfig;
        options.InstanceName = config["Redis:InstanceName"] ?? "app:";
    });

    builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
        ConnectionMultiplexer.Connect(redisConfig));
}

// Hangfire
if (config.GetValue<bool>("Hangfire:UseHangfire"))
{
    builder.Services.AddHangfire(cfg =>
    {
        cfg.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
           .UseSimpleAssemblyNameTypeSerializer()
           .UseRecommendedSerializerSettings()
           .UseSqlServerStorage(config.GetConnectionString("DefaultConnection"), new Hangfire.SqlServer.SqlServerStorageOptions
           {
               CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
               SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
               QueuePollInterval = TimeSpan.FromSeconds(15),
               UseRecommendedIsolationLevel = true,
               DisableGlobalLocks = true
           });
    });
    builder.Services.AddHangfireServer();
}

// CORS
var allowedOrigins = config.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCors", p =>
    {
        if (allowedOrigins.Length == 0)
            p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        else
            p.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
    });
});

// JWT Authentication
var jwtKey = config["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key not configured");
var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = !env.IsDevelopment();
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = config["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = config["Jwt:Audience"],
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromSeconds(30)
    };
});

// Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = " eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJjNmVmYTczZS1kMDhkLTRjNjctYjZlZS0yY2JmYjhhMzFmMWYiLCJlbWFpbCI6Imt1dGF5Y2FuMDFAZ21haWwuY29tIiwibmFtZSI6IiIsImV4cCI6MTc2NTM4NjA2NCwiaXNzIjoiRG9rdGFyUGxhbm5pbmciLCJhdWQiOiJEb2t0YXJQbGFubmluZ0NsaWVudHMifQ.HSZ_srH0dcjrrRuEUfZk-BzU6DothTPx-LUVRbxqiG8",
    });
    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("bearer", document)] = []
    });
});

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseMiddleware<ExceptionHandlerMiddleware>();


app.UseRouting();
app.UseCors("DefaultCors");
app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "DoktarPlanning API v1");
    c.RoutePrefix = "swagger";
});

app.MapControllers();

// Db Migration
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Startup Database Migrate Error");
    }
}

// Hangfire Dashboard
if (config.GetValue<bool>("Hangfire:UseHangfire"))
{
    app.Map("/hangfire", dashboardApp =>
    {
        dashboardApp.UseHangfireDashboard("", new DashboardOptions
        {
            Authorization = new[] { new AllowAllDashboardAuthorizationFilter() }
        });
    });
}
app.Run();
