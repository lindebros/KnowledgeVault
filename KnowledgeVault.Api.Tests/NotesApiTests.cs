using System.Net;
using System.Net.Http.Json;
using KnowledgeVault.Api.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public class NotesApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public NotesApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        db.Database.OpenConnection();
        db.Database.EnsureCreated();
    }

    [Fact]
    public async Task POST_Note_ShouldReturnCreated()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/notes",
            new { title = "Integration Test", content = "Test content" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}