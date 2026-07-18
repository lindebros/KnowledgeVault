using KnowledgeVault.Api.Events.Note;
using KnowledgeVault.Api.Persistence;
using KnowledgeVault.Api.Services;
using KnowledgeVault.Api.Tests.Fakes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

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
        
        var options = Options.Create(new NoteSettings
        {
            MaxTitleLength = 200
        });

        var bus = new FakeEventBus();
        var service = new NoteService(db, NullLogger<NoteService>.Instance, options, bus);

        var result = await service.CreateAsync(
            "Test",
            "Content");

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Contains(bus.PublishedOf<NoteCreatedEvent>(), e => e.NoteId == result.Id);

        var saved = await db.Notes.FirstOrDefaultAsync();

        Assert.NotNull(saved);
        Assert.Equal("Test", saved!.Title);
    }
}