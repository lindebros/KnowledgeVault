using KnowledgeVault.Api.Domain;
using KnowledgeVault.Api.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace KnowledgeVault.Api.Services;

public class NoteService(AppDbContext db,
    ILogger<NoteService> logger,
    IOptions<NoteSettings>  noteSettings)
{
    public async Task<List<Note>> GetAllAsync()
    {
        return await db.Notes.ToListAsync();
    }

    public async Task<Note?> GetByIdAsync(Guid id)
    {
        logger.LogInformation("Getting note {NoteId}", id);
        return await db.Notes.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Note> CreateAsync(string title, string content)
    {
        logger.LogInformation("Creating note with title {Title}", title);
        var exists = await db.Notes.AnyAsync(n => n.Title == title);

        if (exists)
        {
            throw new InvalidOperationException("Title must be unique.");
        }

        if (title.Length > noteSettings.Value.MaxTitleLength)
        {
            throw new InvalidOperationException("Title length must be less than " + noteSettings.Value.MaxTitleLength);
        }
        
        var note = new Note
        {
            Id = Guid.NewGuid(),
            Title = title,
            Content = content,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Notes.Add(note);
        await db.SaveChangesAsync();

        logger.LogInformation("Note create with Id {NoteId}", note.Id);
        return note;
    }
    
    public async Task<Note?> UpdateAsync(Guid id, string title, string content)
    {
        logger.LogInformation("Updating note {NoteId}", id);
        var note = await db.Notes.FindAsync(id);

        if (note == null)
            return null;

        note.Title = title;
        note.Content = content;
        note.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        return note;
    }

    public async Task DeleteAsync(Guid id)
    {
        logger.LogInformation("Deleting note {NoteId}", id);
        var note = await db.Notes.FindAsync(id);
        if (note != null)
        {
            db.Notes.Remove(note);
            await db.SaveChangesAsync();
        }
    }
}