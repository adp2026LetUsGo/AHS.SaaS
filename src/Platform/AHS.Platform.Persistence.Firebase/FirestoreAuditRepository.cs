using System.Net.Http.Json;
using System.Text.Json.Serialization;
using AHS.Platform.Compliance;
using AHS.Common;

namespace AHS.Platform.Persistence.Firebase;

public class FirestoreAuditRepository(HttpClient httpClient, FirebaseOptions options) : IAuditRepository
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly FirebaseOptions _options = options;

    public async Task SaveAsync(AuditRecord record)
    {
        // 1. Sanitize ProjectId (Trim slashes)
        var projectId = _options.ProjectId.Trim('/');
        var id = Guid.NewGuid().ToString();
        
        // 2. Tenant Isolation Path: tenants/{tenantId}/audit_logs/{id}
        // Using relative URL based on https://firestore.googleapis.com/ BaseAddress
        var url = $"v1/projects/{projectId}/databases/(default)/documents/tenants/{record.TenantId}/audit_logs?documentId={id}&key={_options.ApiKey}";

        var firestoreDocument = new FirestoreDocument
        {
            Fields = new Dictionary<string, FirestoreField>
            {
                ["batchId"] = new() { StringValue = record.BatchId },
                ["tenantId"] = new() { StringValue = record.TenantId },
                ["riskScore"] = new() { DoubleValue = record.RiskScore },
                ["hash"] = new() { StringValue = record.Hash },
                ["timestamp"] = new() { StringValue = record.Timestamp.ToString("o") }
            }
        };

        var response = await _httpClient.PostAsJsonAsync(url, firestoreDocument, FirestoreJsonContext.Default.FirestoreDocument);
        response.EnsureSuccessStatusCode();
    }

    public async Task<Result<string>> CheckHealthAsync()
    {
        try
        {
            var projectId = _options.ProjectId.Trim('/');
            if (string.IsNullOrEmpty(projectId)) return Result.Failure<string>("Firebase ProjectId is empty.");

            // Health ping to verify connectivity and API Key
            var url = $"v1/projects/{projectId}/databases/(default)/documents/health_check_ping?key={_options.ApiKey}";
            
            var maskedUrl = $"v1/projects/{projectId}/databases/(default)/documents/health_check_ping?key=***";
            Console.WriteLine($"📡 DIAGNOSTIC: Attempting health check at {maskedUrl}");

            var response = await _httpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorMsg = $"⚠️ HEALTH CHECK FAILED: {response.StatusCode} - {response.ReasonPhrase}";
                Console.WriteLine(errorMsg);
                return Result.Failure<string>(errorMsg);
            }

            return Result.Success("Firebase Connection Healthy");
        }
        catch (Exception ex)
        {
            // Deep diagnostics for Native AOT environments
            Console.WriteLine("==========================================");
            Console.WriteLine($"🔥 REAL EXCEPTION: {ex.GetType().Name}");
            Console.WriteLine($"📝 MESSAGE: {ex.Message}");
            if (ex.InnerException != null) 
            {
                Console.WriteLine($"🔗 INNER: {ex.InnerException.Message}");
            }
            Console.WriteLine($"📍 STACK: {ex.StackTrace}");
            Console.WriteLine("==========================================");
            
            return Result.Failure<string>("Connection failed. Check console logs for details.");
        }
    }
}

public class FirebaseOptions 
{ 
    public string ProjectId { get; set; } = string.Empty; 
    public string ApiKey { get; set; } = string.Empty; 
}

public class FirestoreDocument 
{ 
    [JsonPropertyName("fields")] 
    public Dictionary<string, FirestoreField> Fields { get; set; } = new(); 
}

public class FirestoreField 
{
    [JsonPropertyName("stringValue")] 
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] 
    public string? StringValue { get; set; }
    
    [JsonPropertyName("doubleValue")] 
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] 
    public double? DoubleValue { get; set; }
}

[JsonSerializable(typeof(FirestoreDocument))]
internal partial class FirestoreJsonContext : JsonSerializerContext { }
