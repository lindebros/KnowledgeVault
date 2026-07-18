using System.Net;
using System.Net.Http.Json;
using KnowledgeVault.Api.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public class NotesApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public NotesApiTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
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
    
    [Fact]
    public async Task POST_Note_ShouldPublishOutboxEvent()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/notes",
            new { title = "Integration Tests", content = "Test content" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var sw = System.Diagnostics.Stopwatch.StartNew();
        var published = false;
        while (sw.Elapsed < TimeSpan.FromSeconds(10))
        {
            published = await db.OutboxEvents.AnyAsync(o => o.PublishedAt != null);
            if (published) break;
            await Task.Delay(200);
        }

        Assert.True(published, "Outbox events were not published within timeout");
    }
}