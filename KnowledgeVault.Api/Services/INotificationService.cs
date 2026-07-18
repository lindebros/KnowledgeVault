namespace KnowledgeVault.Api.Services;

public interface INotificationService
{
    Task NotifyAsync(string message, CancellationToken ct = default);
}