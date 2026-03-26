// src/Foundation/AHS.Common/Infrastructure/Persistence/NpgsqlConnectionFactory.cs
using System.Data;
using AHS.Common.Infrastructure.Tenancy;
using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace AHS.Common.Infrastructure.Persistence;

public class NpgsqlConnectionFactory(IConfiguration config, ITenantContext tenant)
    : IDbConnectionFactory
{
    public async Task<IDbConnection> CreateAsync(CancellationToken ct)
    {
        var conn = new NpgsqlConnection(config.GetConnectionString("Default"));
        await conn.OpenAsync(ct).ConfigureAwait(false);
        await conn.ExecuteAsync(
            "SELECT set_config('app.current_tenant_id', @tid, true)",
            new { tid = tenant.TenantId.ToString() }).ConfigureAwait(false);
        return conn;
    }
}
