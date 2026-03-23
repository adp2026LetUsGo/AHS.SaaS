// src/Foundation/AHS.Common/Infrastructure/Tenancy/ITenantContext.cs
namespace AHS.Common.Infrastructure.Tenancy;

public interface ITenantContext
{
    Guid          TenantId      { get; }
    string        TenantSlug    { get; }
    TenantPlan    Plan          { get; }
    IsolationMode IsolationMode { get; }
    string        SchemaName    { get; }   // "public" for Shared, slug for Isolated
}

public enum TenantPlan    { Free, Standard, Pro, Enterprise }
public enum IsolationMode { Shared, Isolated }
