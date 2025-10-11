using System;
using System.Text.Json.Nodes;

namespace CreditPro.Domain.Entities;

public class AuditEvent
{
    public AuditEvent(Guid applicationId, DateTime timestamp, string eventType, string newState, JsonObject details)
    {
        ApplicationId = applicationId;
        Timestamp = timestamp;
        EventType = eventType;
        NewState = newState;
        Details = details;
    }

    public Guid ApplicationId { get; }
    public DateTime Timestamp { get; }
    public string EventType { get; }
    public string NewState { get; }
    public JsonObject Details { get; }
}
