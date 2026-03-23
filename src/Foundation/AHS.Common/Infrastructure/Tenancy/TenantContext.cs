// src/Foundation/AHS.Common/Infrastructure/Tenancy/TenantContext.cs
namespace AHS.Common.Infrastructure.Tenancy;

public record TenantContext : ITenantContext
{
    public Guid          TenantId      { get; init; }
    public string        TenantSlug    { get; init; } = "";
    public TenantPlan    Plan          { get; init; }
    public IsolationMode IsolationMode { get; init; } = IsolationMode.Shared;
    public string        SchemaName    { get; init; } = "public";
}
