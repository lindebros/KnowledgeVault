using KnowledgeVault.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeVault.Api.Contracts.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Note> Notes => Set<Note>();
    public DbSet<NoteTag> NoteTags => Set<NoteTag>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<Audit> Audits => Set<Audit>();
    public DbSet<OutboxEvent> OutboxEvents => Set<OutboxEvent>();
    
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

            entity.HasMany(n => n.NoteTags)
                .WithOne(nt => nt.Note)
                .HasForeignKey(nt => nt.NoteId);
        });
        
        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.HasMany(t => t.NoteTags)
                .WithOne(nt => nt.Tag)
                .HasForeignKey(nt => nt.TagId);
        });

        modelBuilder.Entity<NoteTag>(entity =>
        {
            entity.HasKey(nt => new { nt.NoteId, nt.TagId });

            entity.HasOne(nt => nt.Note)
                .WithMany(n => n.NoteTags)
                .HasForeignKey(nt => nt.NoteId);

            entity.HasOne(nt => nt.Tag)
                .WithMany(t => t.NoteTags)
                .HasForeignKey(nt => nt.TagId);
        });
        
        modelBuilder.Entity<Audit>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.EventType).IsRequired().HasMaxLength(200);
            entity.Property(x => x.Data).IsRequired(false).HasMaxLength(4000);
        });
        
        modelBuilder.Entity<OutboxEvent>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.EventType).IsRequired().HasMaxLength(256);
            e.Property(x => x.Payload).IsRequired();
            e.Property(x => x.Attempts).HasDefaultValue(0);
        });
    }
}