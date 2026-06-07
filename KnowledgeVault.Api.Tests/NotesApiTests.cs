using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace KnowledgeVault.Api.Tests;

public class NotesApiTests(CustomWebApplicationFactory factory) :
    IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task POST_Note_ShouldReturnCreated()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/notes",
            new
            {
                title = "Integration Test",
                content = "Test content"
            });

        Assert.Equal(
            System.Net.HttpStatusCode.Created,
            response.StatusCode);
    }
}