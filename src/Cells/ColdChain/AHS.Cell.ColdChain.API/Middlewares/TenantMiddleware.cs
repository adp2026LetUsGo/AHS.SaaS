using AHS.Common.Infrastructure.Tenancy;
using Microsoft.AspNetCore.Http;

namespace AHS.Cell.ColdChain.API.Middlewares;

public class TenantMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ITenantContextAccessor tenantAccessor)
    {
        if (context.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantIdStr) && 
            Guid.TryParse(tenantIdStr, out var tenantId))
        {
            tenantAccessor.Current = new TenantContext
            {
                TenantId = tenantId,
                TenantSlug = "demo",
                Plan = TenantPlan.Enterprise,
                IsolationMode = IsolationMode.Shared // Default for RLS
            };
        }

        await next(context);
    }
}
