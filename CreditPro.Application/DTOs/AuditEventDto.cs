using System;
using System.Text.Json.Nodes;

namespace CreditPro.Application.DTOs;

public class AuditEventDto
{
    public DateTime Timestamp { get; set; }
    public string EventType { get; set; } = null!;
    public string NewState { get; set; } = null!;
    public JsonObject Details { get; set; } = new();
}
