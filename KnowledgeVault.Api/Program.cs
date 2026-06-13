using KnowledgeVault.Api.Middleware;
using KnowledgeVault.Api.Persistence;
using KnowledgeVault.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeVault.Api
{
    public partial class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            Console.WriteLine(
                $"Environment: {builder.Environment.EnvironmentName}");
            // Controllers
            builder.Services.AddControllers();
            
            // Register primitives
            builder.Services.AddScoped<NoteService>();
            
            // EF Core
            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(
                    builder.Configuration.GetConnectionString("KnowledgeVault"));
            });
            
            builder.Services.Configure<NoteSettings>(
                builder.Configuration.GetSection("NoteSettings"));

            // Swagger
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            app.UseMiddleware<ExceptionHandlingMiddleware>();
            
            // Swagger UI
            if (builder.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            
            if (!app.Environment.IsDevelopment())
            {
                app.UseHsts();
            }

            app.MapControllers();
            
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.Migrate();
            }
            
            app.Run();
        }
    }
}
