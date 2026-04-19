// src/Cells/Xinfer/AHS.Cell.Xinfer.Application/Persistence/Entities/OutboxMessage.cs
using System;

namespace AHS.Cell.Xinfer.Application.Persistence.Entities;

public sealed class OutboxMessage
{
    public Guid           Id          { get; set; }
    public string         EventType   { get; set; } = default!;
    public string         PayloadJson { get; set; } = default!;
    public DateTimeOffset OccurredAt   { get; set; }
    public DateTimeOffset? ProcessedAt { get; set; }
    public string?        Error       { get; set; }
}
