using System.Net;
using System.Net.Http.Json;
using GymProgress.Application.Contracts;

namespace GymProgress.Api.Tests;

public sealed class CoachRecommendationTests : IClassFixture<GymProgressApiFactory>, IAsyncLifetime
{
    private readonly GymProgressApiFactory _factory;
    private HttpClient _client = null!;

    public CoachRecommendationTests(GymProgressApiFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
        _client = _factory.CreateClient();
        await TestAuth.AuthenticateAsync(_client);
    }

    public Task DisposeAsync()
    {
        _client.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task Recommendation_returns_validated_workout_from_mocked_coach()
    {
        var response = await _client.GetAsync("/api/v1/coach/recommendation");
        response.EnsureSuccessStatusCode();

        var recommendation = await response.Content.ReadFromJsonAsync<WorkoutRecommendationDto>();
        Assert.NotNull(recommendation);
        Assert.Equal("Push", recommendation.WorkoutType);
        Assert.NotEmpty(recommendation.CoachNote);
        var exercise = Assert.Single(recommendation.Exercises);
        Assert.Equal("Chest Press", exercise.ExerciseName);
        Assert.Equal(3, exercise.Sets);
        Assert.Equal(8, exercise.TargetRepsMin);
        Assert.Equal(10, exercise.TargetRepsMax);
        Assert.Equal(25, exercise.SuggestedWeight);
        Assert.Equal("maintain", exercise.Progression);
    }
}
