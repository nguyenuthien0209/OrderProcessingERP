using System.Text.Json;

namespace Common.Outbox;

public static class OutboxMessageFactory
{
    public static OutboxMessage Create(object integrationEvent)
    {
        var type = integrationEvent.GetType();
        return new OutboxMessage
        {
            Type = type.AssemblyQualifiedName!,
            Content = JsonSerializer.Serialize(integrationEvent, type),
            OccurredOnUtc = DateTime.UtcNow
        };
    }
}
