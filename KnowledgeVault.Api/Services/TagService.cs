using KnowledgeVault.Api.Contracts.Persistence;
using KnowledgeVault.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeVault.Api.Services;

public class TagService(AppDbContext db,
    ILogger<NoteService> logger)
{
    public async Task<List<Tag>> GetAllAsync()
    {
        return await db.Tags.ToListAsync();
    }
}