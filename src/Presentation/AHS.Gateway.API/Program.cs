using System.Text.Json.Serialization;
using AHS.Gateway.API;
using AHS.Gateway.API.Handlers;
using AHS.Engines.ML;
using AHS.Platform.Compliance;
using AHS.Platform.Persistence.Firebase;
using AHS.Common;

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

    builder.Services.AddSingleton(new ExcursionInferenceService("excursion_risk_v1.onnx"));
    builder.Services.AddScoped<PredictExcursionRiskHandler>();
    builder.Services.AddSingleton<AuditTrailService>();

    builder.Services.AddCors(options => {
        options.AddPolicy("BentoPolicy", policy => {
            policy.WithOrigins("http://localhost:5001", "https://localhost:5001")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
    });

    var app = builder.Build();
    
    app.UseCors("BentoPolicy");
    
    var projectId = builder.Configuration["FIREBASE_PROJECT_ID"];
    Console.WriteLine($"📡 STARTUP: Firebase ProjectId is '{(string.IsNullOrEmpty(projectId) ? "EMPTY" : projectId)}'");

    app.Urls.Add("http://localhost:5000");
    app.MapGet("/", () => "AHS.SaaS Gateway API is Running.");

    app.MapGet("/api/platform/health/firebase", async (IAuditRepository repository) => {
        try {
            var result = await repository.CheckHealthAsync().ConfigureAwait(false);
            if (result.IsSuccess) return Results.Ok(result);
            
            Console.WriteLine($"[ERROR] {result.Error}");
            return Results.Text(result.Error, "text/plain", statusCode: 400);
        } catch (HttpRequestException ex) {
            var error = $"Firebase Connectivity Error: {ex.Message}";
            Console.WriteLine($"[ERROR] {error}");
            return Results.Text(error, "text/plain", statusCode: 400);
        }
    });

    app.MapPost("/api/pharma/traceability/predict-risk", (PredictRiskRequest request, PredictExcursionRiskHandler handler) => {
        try {
            var response = handler.Handle(request);
            return Results.Ok(response);
        } catch (Exception ex) {
            Console.WriteLine($"[ERROR] Prediction Failed: {ex.Message}");
            return Results.Text(ex.Message, "text/plain", statusCode: 400);
        }
    });

    app.Run();
} catch (InvalidOperationException ex) { Console.WriteLine($"[CRITICAL] Startup failed: {ex.Message}"); throw; }

namespace AHS.Gateway.API {
    [JsonSerializable(typeof(PredictRiskRequest))]
    [JsonSerializable(typeof(PredictionResponse))]
    [JsonSerializable(typeof(AHS.Common.Result<PredictionResponse>))]
    [JsonSerializable(typeof(Microsoft.AspNetCore.Mvc.ProblemDetails))]
    [JsonSerializable(typeof(Result<string>))]
    [JsonSerializable(typeof(Result<double>))]
    [JsonSerializable(typeof(string))]
    [JsonSerializable(typeof(bool))]
    internal sealed partial class AppJsonSerializerContext : JsonSerializerContext { }
}
