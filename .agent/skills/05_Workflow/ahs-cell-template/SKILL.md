---
name: ahs-cell-template
description: >
  Universal scaffold and template for any AHS Autonomous Cell (Micro-SaaS unit).
  Use this skill whenever creating a new AHS Cell from scratch, scaffolding a cell
  structure, or verifying a cell follows Blueprint V3.1. Covers project structure,
  required boilerplate, Program.cs AOT setup, JsonSerializerContext, SignedCommand,
  GxP Ledger wiring, Database-per-Cell setup, and the complete NetArchTest suite.
  Trigger on: new cell, scaffold cell, cell template, cell boilerplate, AHS.Cell,
  create cell, cell structure, cell from scratch, cell checklist.
---

# AHS Cell Template — Universal Scaffold
## Blueprint V3.1 / C# 14 / .NET 10 / Native AOT

---

## 1. Solution Structure

```bash
# Create new cell (replace [CellName] throughout)
dotnet new sln -n AHS.Cell.[CellName]
dotnet new classlib -n AHS.Cell.[CellName].Domain         -f net10.0
dotnet new classlib -n AHS.Cell.[CellName].Contracts      -f net10.0
dotnet new classlib -n AHS.Cell.[CellName].Application    -f net10.0
dotnet new classlib -n AHS.Cell.[CellName].Infrastructure -f net10.0
dotnet new web      -n AHS.Cell.[CellName].API             -f net10.0
dotnet new xunit    -n AHS.Cell.[CellName].Tests           -f net10.0
```

---

## 2. Domain Project — Zero Dependencies

```xml
<!-- AHS.Cell.[CellName].Domain.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <!-- NO package references — Domain has ZERO dependencies -->
  </PropertyGroup>
</Project>
```

```csharp
// Domain/Events/DomainEvent.cs — base for all cell events
namespace AHS.Cell.[CellName].Domain.Events;

public abstract record DomainEvent
{
    public Guid           EventId     { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt  { get; init; } = DateTimeOffset.UtcNow;
    public string         EventType   { get; init; } = "";
    public Guid           TenantId    { get; init; }
    public Guid           ActorId     { get; init; }
    public string         ActorName   { get; init; } = "";
    public int            Version     { get; init; } = 1;
}

// Domain/Aggregates/AggregateRoot.cs
namespace AHS.Cell.[CellName].Domain.Aggregates;

public abstract class AggregateRoot
{
    public Guid Id       { get; protected set; }
    public Guid TenantId { get; protected set; }
    public int  Version  { get; private set; }

    private readonly List<DomainEvent> _uncommitted = [];
    public IReadOnlyList<DomainEvent> UncommittedEvents => _uncommitted;

    protected void Apply(DomainEvent evt) { When(evt); _uncommitted.Add(evt); Version++; }
    public void Rehydrate(IEnumerable<DomainEvent> history)
        { foreach (var e in history) { When(e); Version++; } }
    protected abstract void When(DomainEvent evt);
    public void ClearUncommitted() => _uncommitted.Clear();
}

// Application/Commands/SignedCommand.cs
namespace AHS.Cell.[CellName].Application.Commands;

public abstract record SignedCommand
{
    public required Guid   SignedById      { get; init; }
    public required string SignedByName    { get; init; }
    public required string ReasonForChange { get; init; }
    public DateTimeOffset  SignedAt { get; init; } = DateTimeOffset.UtcNow;
    protected SignedCommand()
    {
        if (string.IsNullOrWhiteSpace(ReasonForChange))
            throw new ElectronicSignatureRequiredException(
                "ReasonForChange is mandatory per GxP Integrity guardrail (Blueprint V3.1 G4).");
    }
}

public sealed class ElectronicSignatureRequiredException(string msg) : Exception(msg);
```

---

## 3. API Project — Program.cs (Native AOT)

```csharp
// AHS.Cell.[CellName].API/Program.cs
using AHS.Cell.[CellName].API;
using AHS.Cell.[CellName].Infrastructure;

var builder = WebApplication.CreateSlimBuilder(args);  // AOT-optimized host

// ── JSON (AOT-safe) ──────────────────────────────────────
builder.Services.ConfigureHttpJsonOptions(o =>
    o.SerializerOptions.TypeInfoResolverChain.Insert(0, [CellName]JsonContext.Default));

// ── Auth ─────────────────────────────────────────────────
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("EntraId"));

builder.Services.AddAuthorization(o =>
    o.AddPolicy("SameTenant", p => p.AddRequirements(new TenantIsolationRequirement())));

// ── Infrastructure ────────────────────────────────────────
builder.Services.Add[CellName]Infrastructure(builder.Configuration);

// ── Tenant ───────────────────────────────────────────────
builder.Services.AddScoped<ITenantContextAccessor, TenantContextAccessor>();
builder.Services.AddScoped<ITenantContext>(sp =>
    sp.GetRequiredService<ITenantContextAccessor>().Current
    ?? throw new InvalidOperationException("No tenant context."));

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<TenantResolutionMiddleware>();

// ── Endpoints ─────────────────────────────────────────────
app.MapGroup("/api/[cellname]")
    .Map[CellName]Endpoints()
    .RequireAuthorization("SameTenant");

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
    .AllowAnonymous();

app.Run();

public partial class Program { }  // for WebApplicationFactory in tests
```

---

## 4. JsonSerializerContext (AOT — mandatory)

```csharp
// AHS.Cell.[CellName].API/[CellName]JsonContext.cs
using System.Text.Json.Serialization;

namespace AHS.Cell.[CellName].API;

[JsonSerializable(typeof([CellName]Dto))]
[JsonSerializable(typeof(List<[CellName]SummaryDto>))]
[JsonSerializable(typeof(Create[CellName]Request))]
[JsonSerializable(typeof(ProblemDetails))]
// ← ADD every type that crosses the API boundary
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = false,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
public partial class [CellName]JsonContext : JsonSerializerContext { }
```

---

## 5. Infrastructure Registration

```csharp
// AHS.Cell.[CellName].Infrastructure/InfrastructureExtensions.cs
public static class InfrastructureExtensions
{
    public static IServiceCollection Add[CellName]Infrastructure(
        this IServiceCollection services, IConfiguration config)
    {
        // EF Core 10 + PostgreSQL (write side)
        services.AddDbContext<[CellName]DbContext>(o =>
            o.UseNpgsql(config.GetConnectionString("Default"), npgsql =>
                npgsql.EnableRetryOnFailure(3))
            .AddInterceptors<TenantSessionInterceptor>());

        // Dapper connection factory (read side / CQRS queries)
        services.AddScoped<IDbConnectionFactory, NpgsqlConnectionFactory>();

        // GxP Event Store
        services.AddScoped<IEventStore, PostgresEventStore>();
        services.AddSingleton<LedgerHasher>(sp =>
        {
            var key = Convert.FromBase64String(config["GxPLedger:HmacKeyBase64"]!);
            return new LedgerHasher(key);
        });

        // HybridCache (L1 IMemoryCache + L2 Redis)
        services.AddHybridCache();
        services.AddStackExchangeRedisCache(o =>
            o.Configuration = config["Redis:ConnectionString"]);

        // Service Bus publisher
        services.AddSingleton(new ServiceBusClient(config["ServiceBus:ConnectionString"]));
        services.AddScoped<ICellEventPublisher, ServiceBusCellEventPublisher>();

        // Repositories
        services.AddScoped<I[CellName]Repository, [CellName]Repository>();

        return services;
    }
}
```

---

## 6. PostgreSQL DDL Template

```sql
-- Migration: 001_InitialCreate.sql
-- Cell: [CellName]

-- Main aggregate table
CREATE TABLE [cellname]s (
    id             UUID         NOT NULL DEFAULT gen_random_uuid(),
    tenant_id      UUID         NOT NULL,
    name           VARCHAR(200) NOT NULL,
    status         VARCHAR(50)  NOT NULL DEFAULT 'Active',
    created_at     TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    updated_at     TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    CONSTRAINT pk_[cellname]s PRIMARY KEY (id)
);

-- RLS
ALTER TABLE [cellname]s ENABLE ROW LEVEL SECURITY;
CREATE POLICY tenant_isolation ON [cellname]s
    USING (tenant_id = current_setting('app.current_tenant_id')::uuid);
REVOKE UPDATE, DELETE ON [cellname]s FROM app_role;

-- GxP Ledger
CREATE TABLE ledger_entries (
    sequence       BIGSERIAL    NOT NULL,
    tenant_id      UUID         NOT NULL,
    aggregate_id   UUID         NOT NULL,
    aggregate_type VARCHAR(100) NOT NULL,
    event_type     VARCHAR(200) NOT NULL,
    payload_json   JSONB        NOT NULL,
    actor_id       VARCHAR(100) NOT NULL,
    actor_name     VARCHAR(200) NOT NULL,
    occurred_at    TIMESTAMPTZ  NOT NULL,
    previous_hash  CHAR(64)     NOT NULL,
    entry_hash     CHAR(64)     NOT NULL,
    hmac_seal      VARCHAR(88)  NOT NULL,
    CONSTRAINT pk_ledger PRIMARY KEY (tenant_id, sequence),
    CONSTRAINT uq_ledger_hash UNIQUE (tenant_id, entry_hash)
);
ALTER TABLE ledger_entries ENABLE ROW LEVEL SECURITY;
CREATE POLICY ledger_tenant_isolation ON ledger_entries
    USING (tenant_id = current_setting('app.current_tenant_id')::uuid);
REVOKE UPDATE, DELETE ON ledger_entries FROM app_role;
```

---

## 7. NetArchTest Suite (copy to every cell)

```csharp
// AHS.Cell.[CellName].Tests/Architecture/CellArchitectureTests.cs
public class CellArchitectureTests
{
    [Fact]
    public void Domain_has_zero_external_dependencies()
        => Types.InAssembly(typeof([YourAggregate]).Assembly)
            .Should().NotHaveDependencyOnAny(
                "Npgsql", "Dapper", "Azure", "Microsoft.EntityFrameworkCore",
                "StackExchange.Redis", "System.Net.Http", "System.Text.Json")
            .Because("Blueprint V3.1 G1: Domain has ZERO external dependencies");

    [Fact]
    public void Application_does_not_depend_on_infrastructure()
        => Types.InAssembly(typeof([YourCommandHandler]).Assembly)
            .Should().NotHaveDependencyOnAny("Npgsql", "Azure.Messaging", "StackExchange.Redis")
            .Because("Blueprint V3.1: Application depends only on Domain");

    [Fact]
    public void Domain_models_are_records()
        => Types.InAssembly(typeof([YourAggregate]).Assembly)
            .That().ResideInNamespace("AHS.Cell.[CellName].Domain")
            .And().AreClasses()
            .Should().BeRecord()
            .OrShould().HaveNameEndingWith("Root")  // AggregateRoot exception
            .Because("Blueprint V3.1 G2: all domain models must be record types");

    [Fact]
    public void Write_commands_inherit_SignedCommand()
        => Types.InAssembly(typeof([YourCommand]).Assembly)
            .That().HaveNameEndingWith("Command")
            .And().AreNotAbstract()
            .Should().Inherit(typeof(SignedCommand))
            .Because("Blueprint V3.1 G4: GxP Integrity requires SignedCommand");

    [Fact]
    public void No_reflection_in_domain_or_application()
        => Types.InAssemblies(
                typeof([YourAggregate]).Assembly,
                typeof([YourCommandHandler]).Assembly)
            .Should().NotHaveDependencyOnAny(
                "System.Reflection.Emit", "System.Linq.Expressions")
            .Because("Blueprint V3.1 G1: Native AOT — no reflection or expression compilation");
}
```

---

## 8. Dockerfile (copy to every cell)

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
RUN apt-get update && apt-get install -y clang zlib1g-dev

COPY ["AHS.Cell.[CellName].API/AHS.Cell.[CellName].API.csproj",             "AHS.Cell.[CellName].API/"]
COPY ["AHS.Cell.[CellName].Domain/AHS.Cell.[CellName].Domain.csproj",         "AHS.Cell.[CellName].Domain/"]
COPY ["AHS.Cell.[CellName].Application/AHS.Cell.[CellName].Application.csproj","AHS.Cell.[CellName].Application/"]
COPY ["AHS.Cell.[CellName].Infrastructure/AHS.Cell.[CellName].Infrastructure.csproj","AHS.Cell.[CellName].Infrastructure/"]

RUN dotnet restore "AHS.Cell.[CellName].API/AHS.Cell.[CellName].API.csproj"
COPY . .

RUN dotnet publish "AHS.Cell.[CellName].API/AHS.Cell.[CellName].API.csproj" \
    -r linux-x64 -c Release --no-restore -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime-deps:10.0-noble-chiseled AS final
WORKDIR /app
USER app
COPY --from=build --chown=app:app /app/publish .
EXPOSE 8080
ENTRYPOINT ["/app/AHS.Cell.[CellName].API"]
```
