using AHS.Common.Serialization;
using Microsoft.AspNetCore.Http.Json;

var builder = WebApplication.CreateSlimBuilder(args);

// Native AOT JSON Configuration
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AotJsonContext.Default);
});

// CORS: Allow UI Port (5120) and any method/header for dev friction reduction
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5120")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddControllers();

var app = builder.Build();

app.UseCors(); // Apply policy before mapping controllers

app.MapControllers();

app.Run();
