// src/Foundation/AHS.Common/Infrastructure/Tenancy/ITenantContextAccessor.cs
namespace AHS.Common.Infrastructure.Tenancy;

public interface ITenantContextAccessor
{
    ITenantContext? Current { get; set; }
}
