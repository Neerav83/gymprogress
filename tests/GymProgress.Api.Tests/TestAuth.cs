using System.Net.Http.Headers;
using System.Net.Http.Json;
using GymProgress.Application.Contracts;

namespace GymProgress.Api.Tests;

public static class TestAuth
{
    public static async Task AuthenticateAsync(HttpClient client, string? email = null)
    {
        var response = await client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new RegisterRequest(email ?? $"user-{Guid.NewGuid():N}@gym.test", "password1", "Testare"));
        response.EnsureSuccessStatusCode();
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(auth);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
    }
}
