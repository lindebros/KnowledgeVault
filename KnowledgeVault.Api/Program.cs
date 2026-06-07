using KnowledgeVault.Api.Persistence;
using KnowledgeVault.Api.Repositories;
using KnowledgeVault.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeVault.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Controllers
            builder.Services.AddControllers();
            
            // Register primitives
            builder.Services.AddScoped<NoteService>();
            
            // EF Core
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite("Data Source=knowledgevault.db"));

            // Swagger
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Swagger UI
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpsRedirection();

            app.MapControllers();

            app.Run();
        }
    }
}
