namespace KnowledgeVault.Api.Events;
public class OutboxOptions
{
    public int IntervalSeconds { get; set; } = 5;
    public int BatchSize { get; set; } = 50;
    public int MaxAttempts { get; set; } = 5;
}