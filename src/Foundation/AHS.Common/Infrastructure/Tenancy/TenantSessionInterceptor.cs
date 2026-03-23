// src/Foundation/AHS.Common/Infrastructure/Tenancy/TenantSessionInterceptor.cs
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Npgsql;

namespace AHS.Common.Infrastructure.Tenancy;

public class TenantSessionInterceptor(ITenantContext tenant) : DbCommandInterceptor
{
    public override async ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command, CommandEventData _, InterceptionResult<DbDataReader> result,
        CancellationToken ct)
    {
        await SetTenantContextAsync(command, ct);
        return result;
    }

    public override async ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
        DbCommand command, CommandEventData _, InterceptionResult<int> result,
        CancellationToken ct)
    {
        await SetTenantContextAsync(command, ct);
        return result;
    }

    public override async ValueTask<InterceptionResult<object>> ScalarExecutingAsync(
        DbCommand command, CommandEventData _, InterceptionResult<object> result,
        CancellationToken ct)
    {
        await SetTenantContextAsync(command, ct);
        return result;
    }

    private async Task SetTenantContextAsync(DbCommand command, CancellationToken ct)
    {
        await using var setCmd = command.Connection!.CreateCommand();
        setCmd.Transaction = command.Transaction;

        if (tenant.IsolationMode == IsolationMode.Isolated)
            setCmd.CommandText = $"SET search_path TO {tenant.SchemaName}, public";
        else
        {
            setCmd.CommandText = "SELECT set_config('app.current_tenant_id', @tid, true)";
            setCmd.Parameters.Add(new NpgsqlParameter("tid", tenant.TenantId.ToString()));
        }

        await setCmd.ExecuteNonQueryAsync(ct);
    }
}
