namespace KnowledgeVault.Api.Services;

public class NotificationService : INotificationService
{
    readonly ILogger<NotificationService> _logger;
    public NotificationService(ILogger<NotificationService> logger) => _logger = logger;
    public Task NotifyAsync(string message, CancellationToken ct = default)
    {
        _logger.LogInformation("Notification: {Message}", message);
        return Task.CompletedTask;
    }
}