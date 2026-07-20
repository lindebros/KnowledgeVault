using System.Net;
using System.Net.Http.Json;
using KnowledgeVault.Api.Contracts.Requests;
using KnowledgeVault.Api.Contracts.Responses;
using KnowledgeVault.Api.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace KnowledgeVault.Api.Tests;

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

    [Fact]
    public async Task GET_All_ShouldReturnCreatedNote()
    {
        // Arrange - create a note first
        var createResp = await _client.PostAsJsonAsync("/api/v1/notes", new { title = "ListNote", content = "c" });
        createResp.EnsureSuccessStatusCode();
        var created = await createResp.Content.ReadFromJsonAsync<NoteResponse>();

        // Act
        var resp = await _client.GetAsync("/api/v1/notes");
        resp.EnsureSuccessStatusCode();
        var list = await resp.Content.ReadFromJsonAsync<List<NoteResponse>>();

        // Assert
        Assert.Contains(list!, n => n.Id == created!.Id);
    }

    [Fact]
    public async Task GET_ById_ShouldReturnNote_And_NotFoundWhenMissing()
    {
        // Arrange - create note
        var createResp = await _client.PostAsJsonAsync("/api/v1/notes", new { title = "ById", content = "c" });
        createResp.EnsureSuccessStatusCode();
        var created = await createResp.Content.ReadFromJsonAsync<NoteResponse>();

        // Act - existing id
        var getResp = await _client.GetAsync($"/api/v1/notes/{created!.Id}");
        getResp.EnsureSuccessStatusCode();
        var got = await getResp.Content.ReadFromJsonAsync<NoteResponse>();

        // Assert
        Assert.Equal(created.Id, got!.Id);

        // Act - missing id
        var missing = await _client.GetAsync($"/api/v1/notes/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
    }

    [Fact]
    public async Task PUT_Update_ShouldReturnNoContent_AndModifyNote()
    {
        // Arrange - create
        var createResp = await _client.PostAsJsonAsync("/api/v1/notes", new { title = "ToUpdate", content = "c" });
        createResp.EnsureSuccessStatusCode();
        var created = await createResp.Content.ReadFromJsonAsync<NoteResponse>();

        var updateReq = new UpdateNoteRequest { Title = "Updated", Content = "UpdatedContent" };

        // Act - update existing
        var put = await _client.PutAsJsonAsync($"/api/v1/notes/{created!.Id}", updateReq);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, put.StatusCode);

        var getResp = await _client.GetAsync($"/api/v1/notes/{created.Id}");
        getResp.EnsureSuccessStatusCode();
        var got = await getResp.Content.ReadFromJsonAsync<NoteResponse>();
        Assert.Equal(updateReq.Title, got!.Title);
        Assert.Equal(updateReq.Content, got.Content);

        // Act - update missing id
        var missingPut = await _client.PutAsJsonAsync($"/api/v1/notes/{Guid.NewGuid()}", updateReq);
        Assert.Equal(HttpStatusCode.NotFound, missingPut.StatusCode);
    }

    [Fact]
    public async Task DELETE_ShouldReturnNoContent_AndRemoveNote()
    {
        // Arrange - create
        var createResp = await _client.PostAsJsonAsync("/api/v1/notes", new { title = "ToDeleteApi", content = "c" });
        createResp.EnsureSuccessStatusCode();
        var created = await createResp.Content.ReadFromJsonAsync<NoteResponse>();

        // Act - delete existing
        var del = await _client.DeleteAsync($"/api/v1/notes/{created!.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, del.StatusCode);
        var getAfter = await _client.GetAsync($"/api/v1/notes/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getAfter.StatusCode);

        // Act - delete missing
        var missingDel = await _client.DeleteAsync($"/api/v1/notes/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, missingDel.StatusCode);
    }
}