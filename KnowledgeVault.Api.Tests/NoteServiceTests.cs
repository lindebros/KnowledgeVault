using KnowledgeVault.Api.Persistence;
using KnowledgeVault.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeVault.Api.Tests;

public class NoteServiceTests
{
    private AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task CreateNote_ShouldPersistNote()
    {
        var db = CreateDb();
        var service = new NoteService(db);

        var result = await service.CreateAsync(
            "Test",
            "Content");

        Assert.NotEqual(Guid.Empty, result.Id);

        var saved = await db.Notes.FirstOrDefaultAsync();

        Assert.NotNull(saved);
        Assert.Equal("Test", saved!.Title);
    }
}