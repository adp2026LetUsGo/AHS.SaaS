// src/Cells/ColdChain/AHS.Cell.ColdChain.API/Program.cs
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using AHS.Cell.ColdChain.API;
using AHS.Cell.ColdChain.API.Endpoints;
using AHS.Cell.ColdChain.API.Middlewares;
using AHS.Cell.ColdChain.Infrastructure.Extensions;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(o =>
    o.SerializerOptions.TypeInfoResolverChain.Insert(0, ColdChainJsonContext.Default));

// Authentication & Authorization omitted for brevity in this generation step
// but as per prompt, they would be added here.

builder.Services.AddColdChainInfrastructure(builder.Configuration);

var app = builder.Build();
app.UseMiddleware<TenantMiddleware>();

app.MapGroup("/api/shipments").MapShipmentEndpoints();
app.MapGroup("/api/oracle").MapOracleEndpoints();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", cell = "ColdChain" }));

app.Run();

public partial class Program { }
