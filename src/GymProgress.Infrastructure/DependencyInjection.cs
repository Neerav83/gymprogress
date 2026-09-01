using GymProgress.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace GymProgress.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' saknas.");

        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<AppDbContext>());

        services.Configure<AiOptions>(configuration.GetSection(AiOptions.SectionName));
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddSingleton<ITokenService, JwtTokenService>();
        services.AddHttpClient<IAiCoach, LmStudioCoach>((provider, client) =>
        {
            var options = provider.GetRequiredService<IOptions<AiOptions>>().Value;
            client.BaseAddress = NormalizeBaseAddress(options.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(Math.Clamp(options.TimeoutSeconds, 5, 180));
        });

        return services;
    }

    private static Uri NormalizeBaseAddress(string? baseUrl)
    {
        var value = string.IsNullOrWhiteSpace(baseUrl) ? "http://localhost:1234" : baseUrl.Trim().TrimEnd('/');
        if (!value.EndsWith("/v1", StringComparison.OrdinalIgnoreCase))
        {
            value += "/v1";
        }

        return new Uri(value + "/");
    }
}
