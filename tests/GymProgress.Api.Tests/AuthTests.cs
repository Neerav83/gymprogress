using System.Net;
using System.Net.Http.Json;
using GymProgress.Application.Contracts;

namespace GymProgress.Api.Tests;

public sealed class AuthTests : IClassFixture<GymProgressApiFactory>, IAsyncLifetime
{
    private readonly GymProgressApiFactory _factory;
    private HttpClient _client = null!;

    public AuthTests(GymProgressApiFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
        _client = _factory.CreateClient();
    }

    public Task DisposeAsync()
    {
        _client.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task Workouts_require_authentication()
    {
        var response = await _client.GetAsync("/api/v1/workouts");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Register_and_login_return_token()
    {
        var registered = await _client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new RegisterRequest("tommie@gym.test", "password1", "Tommie"));
        registered.EnsureSuccessStatusCode();
        var created = await registered.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(created);
        Assert.False(string.IsNullOrWhiteSpace(created.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(created.RefreshToken));
        Assert.Equal("tommie@gym.test", created.User.Email);

        var login = await _client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new LoginRequest("tommie@gym.test", "password1"));
        login.EnsureSuccessStatusCode();
        var session = await login.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(session);
        Assert.Equal(created.User.Id, session.User.Id);
    }

    [Fact]
    public async Task Duplicate_email_is_rejected()
    {
        var request = new RegisterRequest("same@gym.test", "password1", "Ett");
        (await _client.PostAsJsonAsync("/api/v1/auth/register", request)).EnsureSuccessStatusCode();
        var duplicate = await _client.PostAsJsonAsync("/api/v1/auth/register", request);
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
    }

    [Fact]
    public async Task Users_cannot_see_each_others_workouts()
    {
        await TestAuth.AuthenticateAsync(_client, "a@gym.test");
        (await _client.PostAsJsonAsync("/api/v1/workouts", new CreateWorkoutRequest(null))).EnsureSuccessStatusCode();
        var mine = await _client.GetFromJsonAsync<List<WorkoutSummaryDto>>("/api/v1/workouts");
        Assert.NotNull(mine);
        Assert.NotEmpty(mine);

        var other = _factory.CreateClient();
        await TestAuth.AuthenticateAsync(other, "b@gym.test");
        var theirs = await other.GetFromJsonAsync<List<WorkoutSummaryDto>>("/api/v1/workouts");
        Assert.NotNull(theirs);
        Assert.Empty(theirs);
    }
}
