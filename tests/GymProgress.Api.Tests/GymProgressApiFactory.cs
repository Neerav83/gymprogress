using GymProgress.Application;
using GymProgress.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;

namespace GymProgress.Api.Tests;

public sealed class GymProgressApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    static GymProgressApiFactory()
    {
        Environment.SetEnvironmentVariable("GYMPROGRESS_SKIP_ENV_FILE", "true");
    }

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17")
        .WithDatabase("gymprogress")
        .WithUsername("gymprogress")
        .WithPassword("gymprogress")
        .Build();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:DefaultConnection", _postgres.GetConnectionString());
        builder.UseSetting("AI:BaseUrl", "http://127.0.0.1:1");
        builder.UseSetting("Jwt:Issuer", "GymProgress");
        builder.UseSetting("Jwt:Audience", "GymProgress");
        builder.UseSetting("Jwt:Key", "GymProgress-test-key-do-not-use-in-prod!!");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.AddDbContext<AppDbContext>(options => options.UseNpgsql(_postgres.GetConnectionString()));
            services.RemoveAll<IAiCoach>();
            services.AddSingleton<IAiCoach, FakeAiCoach>();
        });
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.MigrateAsync();
        await DatabaseInitializer.SeedAsync(db, new Microsoft.Extensions.Logging.Abstractions.NullLogger<AppDbContext>(), CancellationToken.None);
    }
}
