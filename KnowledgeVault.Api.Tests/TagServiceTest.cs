using KnowledgeVault.Api.Contracts.Persistence;
using KnowledgeVault.Api.Contracts.Requests;
using KnowledgeVault.Api.Migrations;
using KnowledgeVault.Api.Services;
using KnowledgeVault.Api.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace KnowledgeVault.Api.Tests;

public class TagServiceTest
{
    private AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private Guid AddNote(AppDbContext db)
    {
        var options = Options.Create(new NoteSettings
        {
            MaxTitleLength = 200
        });

        var service = new NoteService(db, NullLogger<NoteService>.Instance, options);
        
        return service.CreateAsync("One", "Content1").Result.Id;
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllTags()
    {
        // Arrange: create two tags in the in-memory DB.
        var db = CreateDb();
        Guid noteId = AddNote(db);
        
        var tagOptions = Options.Create(new TagSettings { MaxTitleLength = 100 });
        var service = new TagService(db, NullLogger<TagService>.Instance, tagOptions);

        var one = await service.LinkTagToNoteAsync(noteId, new TagRequest { Title = "One" });
        var two = await service.LinkTagToNoteAsync(noteId, new TagRequest { Title = "Two" });

        // Act: call GetAllAsync.
        var all = await service.GetAllAsync();
        // Assert: both tags are returned.
        Assert.Equal(2, all.Count);
        Assert.Contains(all, n => n.Name == one.Name);
        Assert.Contains(all, n => n.Name == two.Name);
    }

    [Fact]
    public async Task GetAllAsync_NoTags_ReturnsEmptyList()
    {
        // Arrange
        var db = CreateDb();
        var tagOptions = Options.Create(new TagSettings { MaxTitleLength = 100 });
        var service = new TagService(db, NullLogger<TagService>.Instance, tagOptions);

        // Act
        var all = await service.GetAllAsync();

        // Assert
        Assert.Empty(all);
    }

    [Fact]
    public async Task GetAllAsync_ShouldProvideNotesOfTags()
    {
        
        // Arrange: create two tags in the in-memory DB.
        var db = CreateDb();
        Guid noteId = AddNote(db);
        
        var tagOptions = Options.Create(new TagSettings { MaxTitleLength = 100 });
        var service = new TagService(db, NullLogger<TagService>.Instance, tagOptions);

        var tag = await service.LinkTagToNoteAsync(noteId, new TagRequest { Title = "Tag" });

        // Act: call GetAllAsync.
        var all = await service.GetAllAsync();
        // Assert: both tags are returned.
        Assert.Single(all);
        Assert.Contains(all, n => n.Name == tag.Name);
        Assert.Contains(all, n => n.Name == tag.Name);
        Assert.Contains(all, n => n.NoteTags.Any(nt => nt.NoteId == noteId));
    }
}