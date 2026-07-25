using KnowledgeVault.Api.Contracts.Persistence;
using KnowledgeVault.Api.Contracts.Requests;
using KnowledgeVault.Api.Domain;
using KnowledgeVault.Api.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace KnowledgeVault.Api.Services;

public class TagService(AppDbContext db,
    ILogger<TagService> logger,
    IOptions<TagSettings> tagSettings, NoteService noteService)
{
    public async Task<List<Tag>> GetAllAsync()
    {
        return await db.Tags.ToListAsync();
    }

    public async Task<Tag> LinkTagToNoteAsync(Guid noteId, TagRequest request)
    {
        if (request.Title.Length > tagSettings.Value.MaxTitleLength)
        {
            throw new InvalidOperationException("Title length must be less than " + tagSettings.Value.MaxTitleLength);
        }
        
        var tag = await db.Tags.Include(tag => tag.NoteTags).FirstOrDefaultAsync(t => t.Name == request.Title);
        
        if (tag == null)
        {
            logger.LogInformation("Creating new tag with title {Title}", request.Title);
            tag = new Tag
            {
                Id = Guid.NewGuid(),
                Name = request.Title
            };
            db.Tags.Add(tag);
            
        }

        var note = await noteService.GetByIdAsync(noteId);

        if (note == null)
        {
            throw new InvalidOperationException("Note not found");
        }
        
        if (tag.NoteTags.Any(nt => nt.NoteId == noteId))
        {
            logger.LogInformation("Tag {Title} is already linked to note {NoteId}", request.Title, noteId);
            return tag;
        }
        
        tag.NoteTags.Add(new NoteTag
        {
            Note = note,
            Tag = tag
        });
        
        await db.SaveChangesAsync();
        return tag;
    }
}