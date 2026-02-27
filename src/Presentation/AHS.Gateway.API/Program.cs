using System.Text.Json.Serialization;
using AHS.Gateway.API;
using AHS.Common;
using AHS.Suites.Pharma.GxP.Traceability.BC.Application.Commands;
using AHS.Suites.Pharma.GxP.Traceability.BC.Application.Handlers;
using AHS.Platform.Compliance;
using AHS.Platform.Persistence.Firebase;

try {
    if (!File.Exists(".env")) Console.WriteLine("⚠️ WARNING: .env file not found at root!");
    EnvLoader.Load(".env");

    var builder = WebApplication.CreateSlimBuilder(args);
    builder.Configuration.AddEnvironmentVariables();
    builder.Services.ConfigureHttpJsonOptions(options => {
        options.SerializerOptions.TypeInfoResolver = AppJsonSerializerContext.Default;
    });

    var firebaseOptions = new FirebaseOptions {
        ProjectId = builder.Configuration["FIREBASE_PROJECT_ID"] ?? string.Empty,
        ApiKey = builder.Configuration["FIREBASE_API_KEY"] ?? string.Empty
    };
    builder.Services.AddSingleton(firebaseOptions);
    
    // Registering IAuditRepository as a Typed Client with BaseAddress
    builder.Services.AddHttpClient<IAuditRepository, FirestoreAuditRepository>(client => {
        client.BaseAddress = new Uri("https://firestore.googleapis.com/");
        Console.WriteLine($"🌐 HttpClient BaseAddress configured: {client.BaseAddress}");
    });

    builder.Services.AddSingleton<PredictExcursionRiskHandler>();
    builder.Services.AddSingleton<AuditTrailService>();

    var app = builder.Build();
    
    var projectId = builder.Configuration["FIREBASE_PROJECT_ID"];
    Console.WriteLine($"📡 STARTUP: Firebase ProjectId is '{(string.IsNullOrEmpty(projectId) ? "EMPTY" : projectId)}'");

    app.Urls.Add("http://localhost:5000");
    app.MapGet("/", () => "AHS.SaaS Gateway API is Running.");

    app.MapGet("/api/platform/health/firebase", async (IAuditRepository repository) => {
        try {
            var result = await repository.CheckHealthAsync();
            if (result.IsSuccess) return Results.Ok(result);
            
            Console.WriteLine($"[ERROR] {result.Error}");
            return Results.Text(result.Error, "text/plain", statusCode: 400);
        } catch (Exception ex) {
            var error = $"Firebase Connectivity Error: {ex.Message}";
            Console.WriteLine($"[ERROR] {error}");
            return Results.Text(error, "text/plain", statusCode: 400);
        }
    });

    app.MapPost("/api/pharma/traceability/predict-risk", async (PredictRiskRequest request, PredictExcursionRiskHandler handler) => {
        var command = new PredictExcursionRiskCommand(request.RouteId, request.Carrier, request.TransitTime, request.AvgTemp, request.Packaging, request.Delay);
        var result = await handler.Handle(command);
        if (!result.IsSuccess) {
            Console.WriteLine($"[ERROR] Prediction Failed: {result.Error}");
            return Results.Text(result.Error, "text/plain", statusCode: 400);
        }
        return Results.Ok(new PredictRiskResponse(result.Value.Score, result.Value.IsHighRisk ? "High Risk" : "Normal", result.Value.IsHighRisk));
    });

    app.Run();
} catch (Exception ex) { Console.WriteLine($"[CRITICAL] Startup failed: {ex.Message}"); throw; }

namespace AHS.Gateway.API {
    public record PredictRiskRequest(string RouteId, string Carrier, int TransitTime, double AvgTemp, string Packaging, bool Delay);
    public record PredictRiskResponse(double Score, string Status, bool IsHighRisk);

    [JsonSerializable(typeof(PredictRiskRequest))]
    [JsonSerializable(typeof(PredictRiskResponse))]
    [JsonSerializable(typeof(PredictionResponseDTO))]
    [JsonSerializable(typeof(Microsoft.AspNetCore.Mvc.ProblemDetails))]
    [JsonSerializable(typeof(Result<string>))]
    [JsonSerializable(typeof(Result<double>))]
    [JsonSerializable(typeof(Result<PredictRiskResponse>))]
    [JsonSerializable(typeof(string))]
    [JsonSerializable(typeof(bool))]
    internal partial class AppJsonSerializerContext : JsonSerializerContext { }
}
