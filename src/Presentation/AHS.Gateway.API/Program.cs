using System.Text.Json.Serialization;
using AHS.Gateway.API;
using AHS.Common;
using AHS.Suites.Pharma.GxP.Traceability.BC.Application.Commands;
using AHS.Suites.Pharma.GxP.Traceability.BC.Application.Handlers;
using AHS.SharedKernel;

try
{
    var builder = WebApplication.CreateSlimBuilder(args);

    // Configure JSON Source Generation for Native AOT (TypeInfoResolver)
    builder.Services.ConfigureHttpJsonOptions(options =>
    {
        options.SerializerOptions.TypeInfoResolver = AppJsonSerializerContext.Default;
    });

    // Register Handler for DI
    builder.Services.AddSingleton<PredictExcursionRiskHandler>();

    var app = builder.Build();

    // Fix the port at http://localhost:5000
    app.Urls.Add("http://localhost:5000");

    // Root Endpoint to verify life
    app.MapGet("/", () => "AHS.SaaS Gateway API is Running.");

    // Pharma Risk Prediction Endpoint
    app.MapPost("/api/pharma/traceability/predict-risk", async (PredictRiskRequest request, PredictExcursionRiskHandler handler) =>
    {
        var command = new PredictExcursionRiskCommand(
            request.RouteId, request.Carrier, request.TransitTime,
            request.AvgTemp, request.Packaging, request.Delay);

        var result = await handler.Handle(command);

        return result.IsSuccess
            ? Results.Ok(new PredictRiskResponse(result.Value.Score, result.Value.IsHighRisk ? "High Risk" : "Normal", result.Value.IsHighRisk))
            : Results.BadRequest(result.Error);
    });

    app.Run();
}
catch (Exception ex)
{
    // Log startup exceptions to console
    Console.WriteLine($"[CRITICAL] Startup failed: {ex.Message}");
    Console.WriteLine(ex.StackTrace);
    throw;
}

namespace AHS.Gateway.API
{
    public record PredictRiskRequest(string RouteId, string Carrier, int TransitTime, double AvgTemp, string Packaging, bool Delay);
    public record PredictRiskResponse(double Score, string Status, bool IsHighRisk);

    // AOT Source Generation Context
    [JsonSerializable(typeof(PredictRiskRequest))]
    [JsonSerializable(typeof(PredictRiskResponse))]
    [JsonSerializable(typeof(PredictionResponseDTO))]
    [JsonSerializable(typeof(string))]
    internal partial class AppJsonSerializerContext : JsonSerializerContext { }
}
