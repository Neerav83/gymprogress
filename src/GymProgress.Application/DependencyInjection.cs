using Microsoft.Extensions.DependencyInjection;

namespace GymProgress.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ExerciseService>();
        services.AddScoped<WorkoutService>();
        services.AddScoped<ProgressService>();
        services.AddScoped<CoachService>();
        services.AddScoped<AuthService>();
        return services;
    }
}
