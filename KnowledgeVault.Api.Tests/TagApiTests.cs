using System.Net;
using System.Net.Http.Json;
using KnowledgeVault.Api.Contracts.Persistence;
using KnowledgeVault.Api.Contracts.Responses;
using KnowledgeVault.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace KnowledgeVault.Api.Tests;

public class TagApiTests : IClassFixture<CustomWebApplicationFactory>
{
    
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;
    
    public TagApiTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        db.Database.OpenConnection();
        db.Database.EnsureCreated();
    }
    
    [Fact]
    public async Task POST_Tag_ShouldReturnCreated()
    {
        var noteResponse = await _client.PostAsJsonAsync(
            "/api/v1/notes",
            new { title = "Integration Test", content = "Test content" });
        var note = await noteResponse.Content.ReadFromJsonAsync<NoteResponse>();
        
        var tagResponse = await _client.PostAsync(
            $"/api/v1/tags?noteId={note!.Id}",
            JsonContent.Create(new { title = "Integration Test Tag" }));
        
        Assert.Equal(HttpStatusCode.Created, tagResponse.StatusCode);
        
        var tag = await tagResponse.Content.ReadFromJsonAsync<TagResponse>();
        Assert.Equal("Integration Test Tag", tag!.Name);
    }
}