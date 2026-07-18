using KnowledgeVault.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeVault.Api.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Note> Notes => Set<Note>();
    public DbSet<Audit> Audits => Set<Audit>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Note>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(x => x.Content)
                .IsRequired()
                .HasMaxLength(5000);
        });
        modelBuilder.Entity<Audit>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.EventType).IsRequired().HasMaxLength(200);
            entity.Property(x => x.Data).IsRequired(false).HasMaxLength(4000);
        });
    }
}