using KnowledgeVault.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System.Text.Json;
using KnowledgeVault.Api.Contracts.Persistence;
using KnowledgeVault.Api.Events.Note;

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
        // Arrange: create in-memory DB, configure NoteSettings, and construct NoteService.
        var db = CreateDb();
        
        var options = Options.Create(new NoteSettings
        {
            MaxTitleLength = 200
        });

        var service = new NoteService(db, NullLogger<NoteService>.Instance, options);

        // Act: call CreateAsync with title and content.
        var result = await service.CreateAsync(
            "Test",
            "Content");

        // Assert: note persisted and outbox event created.
        Assert.NotEqual(Guid.Empty, result.Id);

        var saved = await db.Notes.FirstOrDefaultAsync();
        var outboxEvent = await db.OutboxEvents.FirstOrDefaultAsync();        
        Assert.NotNull(saved);
        Assert.Equal(result.Title, saved!.Title);
        Assert.NotNull(outboxEvent);
        Assert.Contains("NoteCreatedEvent", outboxEvent.EventType);
    }

    [Fact]
    public async Task DeleteAsync_RemovesNoteAndAddsOutboxEvent()
    {
        // Arrange: seed a note via CreateAsync and ensure it exists.
        var db = CreateDb();
        var options = Options.Create(new NoteSettings { MaxTitleLength = 200 });
        var service = new NoteService(db, NullLogger<NoteService>.Instance, options);

        var created = await service.CreateAsync("ToDelete", "Content");
        var exists = await db.Notes.AnyAsync(n => n.Id == created.Id);
        Assert.True(exists);

        // Act: call DeleteAsync on the created id.
        await service.DeleteAsync(created.Id);

        // Assert: note removed and NoteDeletedEvent placed in outbox.
        var after = await db.Notes.FindAsync(created.Id);
        Assert.Null(after);

        var outbox = await db.OutboxEvents.FirstOrDefaultAsync(o => o.EventType.Contains("NoteDeletedEvent"));
        Assert.NotNull(outbox);
        Assert.Contains("NoteDeletedEvent", outbox.EventType);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllNotes()
    {
        // Arrange: create two notes in the in-memory DB.
        var db = CreateDb();
        var options = Options.Create(new NoteSettings { MaxTitleLength = 200 });
        var service = new NoteService(db, NullLogger<NoteService>.Instance, options);

        var one = await service.CreateAsync("One", "Content1");
        var two = await service.CreateAsync("Two", "Content2");

        // Act: call GetAllAsync.
        var all = await service.GetAllAsync();
        // Assert: both notes are returned.
        Assert.Equal(2, all.Count);
        Assert.Contains(all, n => n.Title == one.Title);
        Assert.Contains(all, n => n.Title == two.Title);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNote_WhenExists()
    {
        // Arrange: create a note with CreateAsync.
        var db = CreateDb();
        var options = Options.Create(new NoteSettings { MaxTitleLength = 200 });
        var service = new NoteService(db, NullLogger<NoteService>.Instance, options);

        var created = await service.CreateAsync("FindMe", "SomeContent");
        // Act: call GetByIdAsync with the created id.
        var found = await service.GetByIdAsync(created.Id);

        // Assert: returned note matches the created one.
        Assert.NotNull(found);
        Assert.Equal(created.Title, found!.Title);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateNoteAndAddOutboxEvent()
    {
        // Arrange: create a note and record its original UpdatedAt.
        var db = CreateDb();
        var options = Options.Create(new NoteSettings { MaxTitleLength = 200 });
        var service = new NoteService(db, NullLogger<NoteService>.Instance, options);

        var created = await service.CreateAsync("OldTitle", "OldContent");
        var originalUpdatedAt = created.UpdatedAt;

        // Act: call UpdateAsync to change title and content.
        var updated = await service.UpdateAsync(created.Id, "NewTitle", "NewContent");
        // Assert: note fields updated, UpdatedAt increased, and NoteUpdatedEvent created.
        Assert.NotNull(updated);
        Assert.Equal("NewTitle", updated!.Title);
        Assert.Equal("NewContent", updated.Content);
        Assert.True(updated.UpdatedAt > originalUpdatedAt);

        var outbox = await db.OutboxEvents.FirstOrDefaultAsync(o => o.EventType.Contains("NoteUpdatedEvent"));
        Assert.NotNull(outbox);
        Assert.Contains("NoteUpdatedEvent", outbox.EventType);
    }

    [Fact]
    public async Task CreateAsync_DuplicateTitle_ThrowsInvalidOperationException()
    {
        // Arrange
        var db = CreateDb();
        var options = Options.Create(new NoteSettings { MaxTitleLength = 200 });
        var service = new NoteService(db, NullLogger<NoteService>.Instance, options);

        await service.CreateAsync("Same", "A");

        // Act / Assert: creating with duplicate title throws
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync("Same", "B"));
    }

    [Fact]
    public async Task CreateAsync_TitleExactlyMaxLength_Succeeds()
    {
        // Arrange
        var db = CreateDb();
        var options = Options.Create(new NoteSettings { MaxTitleLength = 5 });
        var service = new NoteService(db, NullLogger<NoteService>.Instance, options);
        var title = new string('x', 5);

        // Act
        var note = await service.CreateAsync(title, "c");

        // Assert
        Assert.NotNull(note);
        Assert.Equal(title, note.Title);
    }

    [Fact]
    public async Task CreateAsync_TitleTooLong_ThrowsInvalidOperationException()
    {
        // Arrange
        var db = CreateDb();
        var options = Options.Create(new NoteSettings { MaxTitleLength = 3 });
        var service = new NoteService(db, NullLogger<NoteService>.Instance, options);
        var title = new string('y', 4);

        // Act / Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(title, "c"));
    }

    [Fact]
    public async Task CreateAsync_NullTitle_Throws()
    {
        // Arrange
        var db = CreateDb();
        var options = Options.Create(new NoteSettings { MaxTitleLength = 200 });
        var service = new NoteService(db, NullLogger<NoteService>.Instance, options);

        // Act / Assert: current implementation will throw a NullReferenceException when title is null
        await Assert.ThrowsAsync<NullReferenceException>(() => service.CreateAsync(null!, "content"));
    }

    [Fact]
    public async Task GetAllAsync_NoNotes_ReturnsEmptyList()
    {
        // Arrange
        var db = CreateDb();
        var options = Options.Create(new NoteSettings { MaxTitleLength = 200 });
        var service = new NoteService(db, NullLogger<NoteService>.Instance, options);

        // Act
        var all = await service.GetAllAsync();

        // Assert
        Assert.Empty(all);
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ReturnsNull()
    {
        // Arrange
        var db = CreateDb();
        var options = Options.Create(new NoteSettings { MaxTitleLength = 200 });
        var service = new NoteService(db, NullLogger<NoteService>.Instance, options);

        // Act
        var found = await service.GetByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(found);
    }

    [Fact]
    public async Task UpdateAsync_NotFound_ReturnsNullAndNoOutboxEvent()
    {
        // Arrange
        var db = CreateDb();
        var options = Options.Create(new NoteSettings { MaxTitleLength = 200 });
        var service = new NoteService(db, NullLogger<NoteService>.Instance, options);

        // Act
        var updated = await service.UpdateAsync(Guid.NewGuid(), "x", "y");

        // Assert
        Assert.Null(updated);
        Assert.False(await db.OutboxEvents.AnyAsync(o => o.EventType.Contains("NoteUpdatedEvent")));
    }

    [Fact]
    public async Task DeleteAsync_NotFound_NoOutboxCreated_NoException()
    {
        // Arrange
        var db = CreateDb();
        var options = Options.Create(new NoteSettings { MaxTitleLength = 200 });
        var service = new NoteService(db, NullLogger<NoteService>.Instance, options);

        // Act (should not throw)
        await service.DeleteAsync(Guid.NewGuid());

        // Assert: no deleted event created
        Assert.False(await db.OutboxEvents.AnyAsync(o => o.EventType.Contains("NoteDeletedEvent")));
    }

    [Fact]
    public async Task OutboxPayloads_ContainCorrectData()
    {
        // Arrange
        var db = CreateDb();
        var options = Options.Create(new NoteSettings { MaxTitleLength = 200 });
        var service = new NoteService(db, NullLogger<NoteService>.Instance, options);

        const string createdTitle = "Ctitle";
        const string createdContent = "Ccontent";
        var created = await service.CreateAsync(createdTitle, createdContent);
        const string updatedTitle = "Utitle";
        const string updatedContent = "Ucontent";
        var updated = await service.UpdateAsync(created.Id, updatedTitle, updatedContent);
        await service.DeleteAsync(created.Id);

        // Act: read outbox events
        var createdOut = await db.OutboxEvents.FirstOrDefaultAsync(o => o.EventType.Contains("NoteCreatedEvent"));
        var updatedOut = await db.OutboxEvents.FirstOrDefaultAsync(o => o.EventType.Contains("NoteUpdatedEvent"));
        var deletedOut = await db.OutboxEvents.FirstOrDefaultAsync(o => o.EventType.Contains("NoteDeletedEvent"));

        // Assert: payloads deserialize and contain expected values
        Assert.NotNull(createdOut);
        var pc = JsonSerializer.Deserialize<NoteCreatedEvent>(createdOut!.Payload);
        Assert.Equal(created.Id, pc!.NoteId);
        Assert.Equal(createdTitle, pc.Title);
        Assert.Equal(createdContent, pc.Content);

        Assert.NotNull(updatedOut);
        var pu = JsonSerializer.Deserialize<NoteUpdatedEvent>(updatedOut!.Payload);
        Assert.Equal(created.Id, pu!.NoteId);
        Assert.Equal(updatedTitle, pu.Title);
        Assert.Equal(updatedContent, pu.Content);

        Assert.NotNull(deletedOut);
        var pd = JsonSerializer.Deserialize<NoteDeletedEvent>(deletedOut!.Payload);
        Assert.Equal(created.Id, pd!.NoteId);
    }
}
