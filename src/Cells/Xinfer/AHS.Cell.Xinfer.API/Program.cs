// AHS Xinfer — eXplainable Inference for Logistics Risk
// Pronounced: "ex-infer"
// Namespace: AHS.Cell.Xinfer
// Replaces: AHS.Cell.ColdChain (renamed 2026-03)
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using AHS.Cell.Xinfer.Application;
using AHS.Cell.Xinfer.Application.Contracts;
using AHS.Cell.Xinfer.API.Endpoints;
using AHS.Cell.Xinfer.API.Middlewares;
using AHS.Cell.Xinfer.Infrastructure.Extensions;
using AHS.Cell.Xinfer.Infrastructure.Services;
using AHS.Common.Infrastructure.Extensions;

[assembly: InternalsVisibleTo("AHS.Cell.Xinfer.Tests")]

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(o =>
    o.SerializerOptions.TypeInfoResolverChain.Insert(0, XinferJsonContext.Default));

builder.Services.AddAhsCommon(builder.Configuration);
builder.Services.AddXinferInfrastructure(builder.Configuration);

// CORS
var allowedOrigins = builder.Configuration.GetSection("CellConfig:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Operational Health & Outbox
builder.Services.AddScoped<XinferHealthService>();
builder.Services.AddHostedService<OutboxPublisherService>();

var app = builder.Build();
app.UseCors();
app.UseMiddleware<TenantMiddleware>();

app.MapGroup("/api/shipments").MapShipmentEndpoints();
app.MapGroup("/api/oracle").MapOracleEndpoints();
app.MapPredictEndpoints();

app.MapGet("/health", () => Results.Ok(new XinferHealthDto { 
    Healthy = true, 
    CellState = XinferLifecycleState.Operational 
}));

app.MapGet("/health/operational", async (XinferHealthService health, CancellationToken ct) =>
{
    var report = await health.GetOperationalHealthAsync(ct).ConfigureAwait(false);
    return report.Healthy ? Results.Ok(report) : Results.StatusCode(503);
});

app.Run();

#pragma warning disable CA1515
public partial class Program { }
#pragma warning restore CA1515
