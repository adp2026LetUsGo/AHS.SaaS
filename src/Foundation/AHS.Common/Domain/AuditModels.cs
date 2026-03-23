// src/Foundation/AHS.Common/Domain/AuditModels.cs
namespace AHS.Common.Domain;

public enum EventSeverity { Info, Warning, Critical }

public record AuditEvent(DateTime Timestamp, string EventName, string ActionTaken, string DigitalSignature, EventSeverity Severity);
