using System.Net;
using System.Net.Http.Json;
using GymProgress.Application.Contracts;

namespace GymProgress.Api.Tests;

public sealed class TemplateTests : IClassFixture<GymProgressApiFactory>, IAsyncLifetime
{
    private readonly GymProgressApiFactory _factory;
    private HttpClient _client = null!;

    public TemplateTests(GymProgressApiFactory factory)
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
    public async Task Template_can_be_created_updated_and_deleted()
    {
        var exercises = await _client.GetFromJsonAsync<List<ExerciseDto>>("/api/v1/exercises");
        Assert.NotNull(exercises);
        var chestPress = exercises.Single(exercise => exercise.Name == "Chest Press");
        var squat = exercises.Single(exercise => exercise.Name == "Squat");

        var created = await _client.PostAsJsonAsync("/api/v1/workouts", new CreateWorkoutRequest(null));
        created.EnsureSuccessStatusCode();
        var workout = await created.Content.ReadFromJsonAsync<WorkoutDto>();
        Assert.NotNull(workout);

        var added = await _client.PostAsJsonAsync(
            $"/api/v1/workouts/{workout.Id}/exercises",
            new AddExerciseRequest(chestPress.Id));
        added.EnsureSuccessStatusCode();

        var templateResponse = await _client.PostAsJsonAsync(
            "/api/v1/workout-templates",
            new CreateTemplateFromWorkoutRequest(workout.Id, "Push", null));
        templateResponse.EnsureSuccessStatusCode();
        var template = await templateResponse.Content.ReadFromJsonAsync<WorkoutTemplateDto>();
        Assert.NotNull(template);
        Assert.Equal("Push", template.Name);
        Assert.Single(template.Exercises);
        Assert.Equal(chestPress.Id, template.Exercises[0].ExerciseId);

        var updatedResponse = await _client.PutAsJsonAsync(
            $"/api/v1/workout-templates/{template.Id}",
            new UpdateWorkoutTemplateRequest("Ben", "Tunga baslyft", [squat.Id, chestPress.Id]));
        updatedResponse.EnsureSuccessStatusCode();
        var updated = await updatedResponse.Content.ReadFromJsonAsync<WorkoutTemplateDto>();
        Assert.NotNull(updated);
        Assert.Equal("Ben", updated.Name);
        Assert.Equal("Tunga baslyft", updated.Description);
        Assert.Equal(2, updated.Exercises.Count);
        Assert.Equal(squat.Id, updated.Exercises[0].ExerciseId);
        Assert.Equal(chestPress.Id, updated.Exercises[1].ExerciseId);

        var fetched = await _client.GetFromJsonAsync<WorkoutTemplateDto>($"/api/v1/workout-templates/{template.Id}");
        Assert.NotNull(fetched);
        Assert.Equal("Ben", fetched.Name);

        var deleted = await _client.DeleteAsync($"/api/v1/workout-templates/{template.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);
        var missing = await _client.GetAsync($"/api/v1/workout-templates/{template.Id}");
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
    }
}
