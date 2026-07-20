using KnowledgeVault.Api.Contracts.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace KnowledgeVault.Api.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private PostgreSqlContainer _container;

    public CustomWebApplicationFactory()
    {
        _container = new PostgreSqlBuilder()
            .WithImage("postgres:16")
            .WithDatabase("tests")           
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();
        _container.StartAsync().GetAwaiter().GetResult();
    }
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((ctx, cfg) =>
        {
            cfg.AddInMemoryCollection(new Dictionary<string, string>
            {
                ["Outbox:IntervalSeconds"] = "1",
                ["Outbox:BatchSize"] = "10",
                ["Outbox:MaxAttempts"] = "3",
                ["Diagnostics:ShowExceptionDetails"] = "true",
                ["ConnectionStrings:KnowledgeVault"] = _container.GetConnectionString()
            });
        });

        // Let the application register DbContext using the connection string above so Migrate/EnsureCreated
        // run against the PostgreSQL test container. Avoid re-registering the DbContext here to prevent
        // multiple provider registrations.
    }

    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Ensure the relational schema exists for the PostgreSQL test container.
        db.Database.EnsureCreated();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}