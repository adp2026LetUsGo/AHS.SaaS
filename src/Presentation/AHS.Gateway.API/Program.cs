using AHS.Common.Serialization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("ReflectionAnalysis", "IL2026:RequiresUnreferencedCode", Justification = "Controllers are registered via TypeInfoResolverChain and AotJsonContext.")]

var builder = WebApplication.CreateSlimBuilder(args);

// PRODUCTION_READY_CORS: Dynamic Origins from Environment
var allowedOrigins = Environment.GetEnvironmentVariable("ALLOWED_ORIGINS")?.Split(',') ?? new[] { "http://localhost:5120" };

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// NATIVE AOT JSON CONFIGURATION
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.TypeInfoResolverChain.Insert(0, AotJsonContext.Default);
    });

var app = builder.Build();

app.UseCors();
app.MapControllers();

app.Run();
