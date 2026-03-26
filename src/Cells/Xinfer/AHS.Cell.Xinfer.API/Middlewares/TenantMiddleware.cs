using AHS.Common.Infrastructure.Tenancy;
using Microsoft.AspNetCore.Http;

using System.Diagnostics.CodeAnalysis;

namespace AHS.Cell.Xinfer.API.Middlewares;

[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated by ASP.NET Core middleware pipeline")]
internal sealed class TenantMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ITenantContextAccessor tenantAccessor)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(tenantAccessor);

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

        await next(context).ConfigureAwait(false);
    }
}
