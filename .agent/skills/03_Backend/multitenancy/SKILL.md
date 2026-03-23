---
name: multitenancy
description: >
  Expert guidance on designing and implementing multi-tenant architectures in C#/.NET.
  Use this skill whenever the user mentions tenants, tenant isolation, SaaS architecture,
  per-tenant configuration, tenant resolution, database-per-tenant, shared schema,
  tenant middleware, ITenantContext, or anything related to serving multiple customers
  from a single application. Trigger on keywords: multitenancy, multi-tenant, tenant,
  SaaS, tenant ID, tenant resolver, tenant database, row-level security, tenant scoping.
---

# Multitenancy in C# / .NET

## Core Tenant Models

| Model | Isolation | Cost | Complexity |
|---|---|---|---|
| Database-per-tenant | High | High | Medium |
| Schema-per-tenant | Medium | Medium | Medium |
| Shared schema (discriminator column) | Low | Low | Low |
| Hybrid (shared + isolated) | Configurable | Flexible | High |

---

## 0. Stack de Base de Datos (AHS)

```xml
<!-- .csproj -->
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="10.*" />
<PackageReference Include="Dapper" Version="2.*" />
<!-- Para Dapper + PostgreSQL -->
<PackageReference Include="Npgsql" Version="9.*" />
```

```csharp
// Program.cs — EF Core 10 + PostgreSQL
builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseNpgsql(builder.Configuration.GetConnectionString("Default"), npgsql => npgsql
        .EnableRetryOnFailure(3)
        .MigrationsHistoryTable("__ef_migrations", schema: "ahs")));

// Dapper — para queries de lectura en projections (CQRS read side)
builder.Services.AddScoped<IDbConnectionFactory, NpgsqlConnectionFactory>();
```

```csharp
// AOT-safe Dapper connection factory
public class NpgsqlConnectionFactory(IConfiguration config, ITenantContext tenant)
    : IDbConnectionFactory
{
    public async Task<IDbConnection> CreateAsync(CancellationToken ct)
    {
        var conn = new NpgsqlConnection(config.GetConnectionString("Default"));
        await conn.OpenAsync(ct);
        // Set tenant RLS context immediately on open
        await conn.ExecuteAsync(
            "SELECT set_config('app.current_tenant_id', @tid, true)",
            new { tid = tenant.TenantId.ToString() });
        return conn;
    }
}
```

---

### ITenantContext (the backbone)

```csharp
public interface ITenantContext
{
    Guid TenantId { get; }
    string TenantSlug { get; }
    TenantPlan Plan { get; }
}

public class TenantContext : ITenantContext
{
    public Guid TenantId { get; init; }
    public string TenantSlug { get; init; } = default!;
    public TenantPlan Plan { get; init; }
}
```

### Tenant Resolver Middleware

```csharp
public class TenantResolutionMiddleware(RequestDelegate next, ITenantResolver resolver)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var tenant = await resolver.ResolveAsync(context);
        if (tenant is null)
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        context.Items["TenantContext"] = tenant;

        // Also set in DI scope
        var tenantAccessor = context.RequestServices.GetRequiredService<ITenantContextAccessor>();
        tenantAccessor.Current = tenant;

        await next(context);
    }
}
```

### Resolver Strategies

```csharp
// Host-based: tenant1.myapp.com
public class HostTenantResolver : ITenantResolver
{
    public Task<ITenantContext?> ResolveAsync(HttpContext ctx)
    {
        var host = ctx.Request.Host.Host; // "tenant1.myapp.com"
        var slug = host.Split('.')[0];
        return _repo.FindBySlugAsync(slug);
    }
}

// Header-based: X-Tenant-Id
public class HeaderTenantResolver : ITenantResolver
{
    public Task<ITenantContext?> ResolveAsync(HttpContext ctx)
    {
        if (!ctx.Request.Headers.TryGetValue("X-Tenant-Id", out var id))
            return Task.FromResult<ITenantContext?>(null);
        return _repo.FindByIdAsync(Guid.Parse(id!));
    }
}

// Route-based: /api/{tenantSlug}/orders
public class RouteTenantResolver : ITenantResolver
{
    public Task<ITenantContext?> ResolveAsync(HttpContext ctx)
    {
        var slug = ctx.GetRouteValue("tenantSlug")?.ToString();
        return slug is null ? Task.FromResult<ITenantContext?>(null)
                            : _repo.FindBySlugAsync(slug);
    }
}
```

---

## 2. DI Registration

```csharp
// ITenantContextAccessor — scoped, one per request
public class TenantContextAccessor : ITenantContextAccessor
{
    public ITenantContext? Current { get; set; }
}

// Program.cs
builder.Services.AddScoped<ITenantContextAccessor, TenantContextAccessor>();
builder.Services.AddScoped<ITenantContext>(sp =>
    sp.GetRequiredService<ITenantContextAccessor>().Current
    ?? throw new InvalidOperationException("No tenant context for this request."));
```

---

## 3. EF Core — Shared Schema (Global Query Filter)

> ⚠️ **AOT / Trim:** EF Core 9/10 es compatible con Native AOT en modo "no-reflection". Evita `modelBuilder.Model.GetEntityTypes()` + `MakeGenericMethod()` — registra filtros explícitamente por entidad.

```csharp
public class AppDbContext(DbContextOptions options, ITenantContext tenant)
    : DbContext(options)
{
    private readonly Guid _tenantId = tenant.TenantId;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ✅ AOT-safe: registrar filtros explícitamente por tipo, sin reflection.
        // El patrón MakeGenericMethod() con BindingFlags rompe en Native AOT.
        // EF Core 8+ ya aplica HasQueryFilter con expresiones lambda — sin reflection en runtime.
        modelBuilder.Entity<Order>().HasQueryFilter(e => e.TenantId == _tenantId);
        modelBuilder.Entity<Invoice>().HasQueryFilter(e => e.TenantId == _tenantId);
        modelBuilder.Entity<Product>().HasQueryFilter(e => e.TenantId == _tenantId);
        // Agregar una línea por cada entidad ITenantScoped.

        // ℹ️  Si necesitas hacerlo genérico sin reflection, usa una interfaz + método virtual:
        // mb.ApplyConfigurationsFromAssembly() también es AOT-safe (usa IEntityTypeConfiguration<T>)
    }

    // ❌ EVITAR en AOT — MakeGenericMethod + BindingFlags no funciona en Native AOT:
    // var method = GetType().GetMethod("X", BindingFlags.NonPublic | BindingFlags.Static)!
    //              .MakeGenericMethod(entityType.ClrType);
    // method.Invoke(null, args);   ← IL2060 trim warning → crash en AOT
}

public interface ITenantScoped
{
    Guid TenantId { get; }
}
```

---

## 4. EF Core — Database-per-Tenant (PostgreSQL / Npgsql)

```csharp
// Resolve connection string per tenant
public class TenantDbContextFactory(ITenantContext tenant, IConfiguration config)
{
    public AppDbContext Create()
    {
        var connStr = config.GetConnectionString($"Tenant_{tenant.TenantSlug}")
                   ?? config.GetConnectionString("DefaultTenant")!;

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connStr, o => o
                .EnableRetryOnFailure(maxRetryCount: 3)
                .CommandTimeout(30))
            .Options;

        return new AppDbContext(options, tenant);
    }
}

// Connection string format (PostgreSQL):
// "Host=ahs-db.postgres.database.azure.com;Database=ahs_tenant1;
//  Username=ahs_app;Password=...;SSL Mode=Require;Trust Server Certificate=false"
```

---

## 5. Per-Tenant Configuration with IOptions

```csharp
// appsettings.json structure:
// "Tenants": { "acme": { "MaxUsers": 100 }, "bigcorp": { "MaxUsers": 5000 } }

public class TenantOptions
{
    public int MaxUsers { get; set; }
    public string StorageContainer { get; set; } = "";
    public bool EnableFeatureX { get; set; }
}

// Resolve at runtime via ITenantContext
public class TenantOptionsProvider(IConfiguration config, ITenantContext tenant)
{
    public TenantOptions GetOptions()
    {
        var section = config.GetSection($"Tenants:{tenant.TenantSlug}");
        return section.Exists()
            ? section.Get<TenantOptions>()!
            : config.GetSection("Tenants:Default").Get<TenantOptions>()!;
    }
}
```

---

## 6. Authorization — Tenant Isolation via Policy

```csharp
public class TenantIsolationRequirement : IAuthorizationRequirement { }

public class TenantIsolationHandler(ITenantContext tenant)
    : AuthorizationHandler<TenantIsolationRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext ctx,
        TenantIsolationRequirement requirement)
    {
        var claimTenantId = ctx.User.FindFirstValue("tenant_id");
        if (claimTenantId == tenant.TenantId.ToString())
            ctx.Succeed(requirement);
        return Task.CompletedTask;
    }
}

// Registration
builder.Services.AddAuthorization(o =>
    o.AddPolicy("SameTenant", p => p.AddRequirements(new TenantIsolationRequirement())));
```

---

## 7. Row-Level Security (PostgreSQL)

```sql
-- PostgreSQL RLS — más limpio que SQL Server, sin sp_set_session_context
ALTER TABLE orders ENABLE ROW LEVEL SECURITY;

CREATE POLICY tenant_isolation ON orders
    USING (tenant_id = current_setting('app.current_tenant_id')::uuid);

-- La app_role solo puede ver sus propias filas
GRANT SELECT, INSERT ON orders TO app_role;
-- No GRANT UPDATE/DELETE para las filas de otros tenants — RLS lo bloquea
```

```csharp
// Interceptor para PostgreSQL — set_config en lugar de sp_set_session_context
public class TenantSessionInterceptor(ITenantContext tenant) : DbCommandInterceptor
{
    public override async ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result,
        CancellationToken ct)
    {
        // PostgreSQL: set_config es la forma estándar (equivalente al session context de SQL Server)
        await using var setCmd = command.Connection!.CreateCommand();
        setCmd.Transaction = command.Transaction;
        setCmd.CommandText = "SELECT set_config('app.current_tenant_id', @tid, true)";
        setCmd.Parameters.Add(new NpgsqlParameter("tid", tenant.TenantId.ToString()));
        await setCmd.ExecuteNonQueryAsync(ct);
        return result;
    }
}
```

---

## 8. Background Jobs — Tenant Context in Hangfire/Worker

```csharp
// Inject TenantId into job payload, not via ambient context
public record ProcessOrdersJob(Guid TenantId);

public class ProcessOrdersHandler(ITenantContextFactory factory)
{
    public async Task Handle(ProcessOrdersJob job, CancellationToken ct)
    {
        // Reconstruct tenant context from payload
        using var scope = _sp.CreateScope();
        var accessor = scope.ServiceProvider.GetRequiredService<ITenantContextAccessor>();
        accessor.Current = await factory.LoadAsync(job.TenantId, ct);

        var orders = scope.ServiceProvider.GetRequiredService<IOrderService>();
        await orders.ProcessPendingAsync(ct);
    }
}
```

---

## Common Pitfalls

| Pitfall | Solution |
|---|---|
| `ITenantContext` resolved outside request scope | Use `ITenantContextAccessor`, not direct injection |
| Background jobs losing tenant | Always serialize `TenantId` into job payload |
| Cross-tenant data leak | Global Query Filters + integration tests asserting isolation |
| Missing migrations per tenant | Use `IMigrator` per connection string in startup |
| N+1 per-tenant queries | Cache `TenantOptions` with `IMemoryCache` keyed by `TenantId` |

---

## Testing Isolation

```csharp
// Integration test asserting tenant A cannot see tenant B's data
[Fact]
public async Task TenantA_CannotSee_TenantB_Orders()
{
    var contextA = new TenantContext { TenantId = _tenantA };
    var dbA = CreateDbContext(contextA);

    var order = await dbA.Orders.FirstOrDefaultAsync(o => o.TenantId == _tenantB);
    Assert.Null(order); // Global filter must block this
}
```
