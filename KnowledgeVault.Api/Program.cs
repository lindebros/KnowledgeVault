using KnowledgeVault.Api.Events;
using KnowledgeVault.Api.Events.Handlers;
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
            builder.Services.AddHostedService<OutboxPublisher>();
            builder.Services.AddMemoryCache();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            
            // Event bus
            builder.Services.AddSingleton<IEventBus, InMemoryEventBus>();
            builder.Services.AddTransient<NoteCreatedEventHandler>();
            builder.Services.AddTransient<NoteUpdatedEventHandler>();
            builder.Services.AddTransient<NoteDeletedEventHandler>();
            
            // EF Core
            if (builder.Environment.IsEnvironment("Testing"))
            {
                builder.Services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlite("DataSource=:memory:"));
            }
            else
            {
                builder.Services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseNpgsql(
                        builder.Configuration.GetConnectionString("KnowledgeVault"));
                });
            }
            
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
                var bus = scope.ServiceProvider.GetRequiredService<IEventBus>();
                var provider = scope.ServiceProvider;
                var handlerTypes = typeof(Program).Assembly.GetTypes()
                    .Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventHandler<>)));
                foreach (var type in handlerTypes)
                {
                    foreach (var iface in type.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventHandler<>)))
                    {
                        var eventType = iface.GetGenericArguments()[0];
                        var handler = provider.GetService(type);
                        if (handler == null) continue;
                        var subscribeMethod = typeof(IEventBus).GetMethod("Subscribe")!.MakeGenericMethod(eventType);
                        subscribeMethod.Invoke(bus, new[] { handler });
                    }
                }
                
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.Migrate();
            }
            
            app.Run();
        }
    }
}
