// src/Foundation/AHS.Common/Infrastructure/Tenancy/TenantContextAccessor.cs
namespace AHS.Common.Infrastructure.Tenancy;

public class TenantContextAccessor : ITenantContextAccessor
{
    public ITenantContext? Current { get; set; }
}
