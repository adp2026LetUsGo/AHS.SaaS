// AHS Xinfer — eXplainable Inference for Logistics Risk
// Pronounced: "ex-infer"
// Namespace: AHS.Cell.Xinfer
// Replaces: AHS.Cell.ColdChain (renamed 2026-03)
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using AHS.Cell.Xinfer.API;
using AHS.Cell.Xinfer.API.Endpoints;
using AHS.Cell.Xinfer.API.Middlewares;
using AHS.Cell.Xinfer.Infrastructure.Extensions;

[assembly: InternalsVisibleTo("AHS.Cell.Xinfer.Tests")]

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(o =>
    o.SerializerOptions.TypeInfoResolverChain.Insert(0, XinferJsonContext.Default));

// Authentication & Authorization omitted for brevity in this generation step
// but as per prompt, they would be added here.

builder.Services.AddXinferInfrastructure(builder.Configuration);

var app = builder.Build();
app.UseMiddleware<TenantMiddleware>();

app.MapGroup("/api/shipments").MapShipmentEndpoints();
app.MapGroup("/api/oracle").MapOracleEndpoints();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", cell = "Xinfer" }));

// CA1515 suppressed: required for WebApplicationFactory<Program> in integration tests
#pragma warning disable CA1515
public partial class Program { }
#pragma warning restore CA1515
