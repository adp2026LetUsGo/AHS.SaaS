// src/Foundation/AHS.Common/Infrastructure/Extensions/CommonServiceExtensions.cs
using AHS.Common.Infrastructure.GxP;
using AHS.Common.Infrastructure.Persistence;
using AHS.Common.Infrastructure.Tenancy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AHS.Common.Infrastructure.Extensions;

public static class CommonServiceExtensions
{
    public static IServiceCollection AddAhsCommon(
        this IServiceCollection services,
        IConfiguration config)
    {
        // Tenant
        services.AddScoped<ITenantContextAccessor, TenantContextAccessor>();
        services.AddScoped<ITenantContext>(sp =>
            sp.GetRequiredService<ITenantContextAccessor>().Current
            ?? throw new InvalidOperationException("No tenant context for this request."));
        services.AddScoped<TenantSessionInterceptor>();

        // Persistence
        services.AddScoped<IDbConnectionFactory, NpgsqlConnectionFactory>();

        // GxP Ledger key from Key Vault (resolved at startup)
        services.AddSingleton<LedgerHasher>(sp =>
        {
            var key = Convert.FromBase64String(
                config["GxPLedger:HmacKeyBase64"]
                ?? throw new InvalidOperationException("GxP HMAC key not configured."));
            return new LedgerHasher(key);
        });

        return services;
    }
}
