using System.Net;
using System.Net.Http.Json;
using GymProgress.Application.Contracts;

namespace GymProgress.Api.Tests;

public sealed class WorkoutFlowTests : IClassFixture<GymProgressApiFactory>, IAsyncLifetime
{
    private readonly GymProgressApiFactory _factory;
    private HttpClient _client = null!;

    public WorkoutFlowTests(GymProgressApiFactory factory)
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
    public async Task Workout_logging_flow_persists_sets_and_history()
    {
        var exercises = await _client.GetFromJsonAsync<List<ExerciseDto>>("/api/v1/exercises");
        Assert.NotNull(exercises);
        Assert.Contains(exercises, exercise => exercise.Name == "Chest Press");
        var chestPress = exercises.Single(exercise => exercise.Name == "Chest Press");

        var created = await _client.PostAsJsonAsync("/api/v1/workouts", new CreateWorkoutRequest(null));
        created.EnsureSuccessStatusCode();
        var workout = await created.Content.ReadFromJsonAsync<WorkoutDto>();
        Assert.NotNull(workout);
        Assert.True(workout.IsActive);

        var addedExercise = await _client.PostAsJsonAsync(
            $"/api/v1/workouts/{workout.Id}/exercises",
            new AddExerciseRequest(chestPress.Id));
        addedExercise.EnsureSuccessStatusCode();
        var workoutExercise = await addedExercise.Content.ReadFromJsonAsync<WorkoutExerciseDto>();
        Assert.NotNull(workoutExercise);

        var firstSet = await _client.PostAsJsonAsync(
            $"/api/v1/workouts/{workout.Id}/exercises/{workoutExercise.Id}/sets",
            new AddSetRequest(25, 10));
        firstSet.EnsureSuccessStatusCode();
        var firstResult = await firstSet.Content.ReadFromJsonAsync<AddSetResponse>();
        Assert.NotNull(firstResult);
        Assert.Equal(25, firstResult.Set.WeightKg);
        Assert.Equal(10, firstResult.Set.Reps);
        Assert.Empty(firstResult.PersonalRecords);

        var secondSet = await _client.PostAsJsonAsync(
            $"/api/v1/workouts/{workout.Id}/exercises/{workoutExercise.Id}/sets",
            new AddSetRequest(25, 10));
        secondSet.EnsureSuccessStatusCode();

        var thirdSet = await _client.PostAsJsonAsync(
            $"/api/v1/workouts/{workout.Id}/exercises/{workoutExercise.Id}/sets",
            new AddSetRequest(25, 5));
        thirdSet.EnsureSuccessStatusCode();

        var finished = await _client.PostAsync($"/api/v1/workouts/{workout.Id}/finish", null);
        finished.EnsureSuccessStatusCode();
        var completed = await finished.Content.ReadFromJsonAsync<WorkoutDto>();
        Assert.NotNull(completed);
        Assert.False(completed.IsActive);
        Assert.Equal(625, completed.TotalVolumeKg);

        var history = await _client.GetFromJsonAsync<List<WorkoutSummaryDto>>("/api/v1/workouts");
        Assert.NotNull(history);
        Assert.Contains(history, item => item.Id == workout.Id && item.SetCount == 3);

        var progress = await _client.GetFromJsonAsync<ExerciseProgressDto>($"/api/v1/progress/{chestPress.Id}?range=all");
        Assert.NotNull(progress);
        Assert.NotEmpty(progress.Points);
        Assert.Equal(25, progress.Points[0].MaxWeightKg);

        var records = await _client.GetFromJsonAsync<List<PersonalRecordDto>>("/api/v1/personal-records");
        Assert.NotNull(records);
        Assert.Contains(records, record => record.ExerciseName == "Chest Press");

        var next = await _client.PostAsJsonAsync("/api/v1/workouts", new CreateWorkoutRequest(null));
        next.EnsureSuccessStatusCode();
        var secondWorkout = await next.Content.ReadFromJsonAsync<WorkoutDto>();
        Assert.NotNull(secondWorkout);

        var nextExerciseResponse = await _client.PostAsJsonAsync(
            $"/api/v1/workouts/{secondWorkout.Id}/exercises",
            new AddExerciseRequest(chestPress.Id));
        nextExerciseResponse.EnsureSuccessStatusCode();
        var nextExercise = await nextExerciseResponse.Content.ReadFromJsonAsync<WorkoutExerciseDto>();
        Assert.NotNull(nextExercise);
        Assert.NotNull(nextExercise.LastSession);
        Assert.Equal(3, nextExercise.LastSession.Sets.Count);

        var prSet = await _client.PostAsJsonAsync(
            $"/api/v1/workouts/{secondWorkout.Id}/exercises/{nextExercise.Id}/sets",
            new AddSetRequest(27.5m, 10));
        prSet.EnsureSuccessStatusCode();
        var prResult = await prSet.Content.ReadFromJsonAsync<AddSetResponse>();
        Assert.NotNull(prResult);
        Assert.Contains(prResult.PersonalRecords, record => record.Type == "HighestWeight");

        var deleted = await _client.DeleteAsync($"/api/v1/workouts/{secondWorkout.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);
        var missing = await _client.GetAsync($"/api/v1/workouts/{secondWorkout.Id}");
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
    }

    [Fact]
    public async Task Health_endpoint_is_available()
    {
        var response = await _client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
