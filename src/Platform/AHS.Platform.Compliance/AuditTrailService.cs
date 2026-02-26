using System.Text.Json;
using System.Text.Json.Serialization;

namespace AHS.Platform.Compliance;

public record AuditLog(DateTime Timestamp, string Action, string Data, string Hash);

public static class AuditTrailService
{
    private const string AuditFilePath = "audit_trail.json";

    public static async Task LogAsync(string action, string data, string hash)
    {
        var entry = new AuditLog(DateTime.UtcNow, action, data, hash);
        var json = JsonSerializer.Serialize(entry, AuditJsonContext.Default.AuditLog);
        
        // Simple append for monorepo demonstration
        await File.AppendAllTextAsync(AuditFilePath, json + Environment.NewLine);
    }
}

[JsonSourceGenerationOptions(WriteIndented = false)]
[JsonSerializable(typeof(AuditLog))]
internal partial class AuditJsonContext : JsonSerializerContext { }
