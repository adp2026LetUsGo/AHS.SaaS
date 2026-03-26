// src/Foundation/AHS.Common/Infrastructure/Tenancy/TenantSessionInterceptor.cs
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Npgsql;

namespace AHS.Common.Infrastructure.Tenancy;

public class TenantSessionInterceptor(ITenantContext tenant) : DbCommandInterceptor
{
    public override async ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        await SetTenantContextAsync(command, cancellationToken).ConfigureAwait(false);
        return result;
    }

    public override async ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
        DbCommand command, CommandEventData eventData, InterceptionResult<int> result,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        await SetTenantContextAsync(command, cancellationToken).ConfigureAwait(false);
        return result;
    }

    public override async ValueTask<InterceptionResult<object>> ScalarExecutingAsync(
        DbCommand command, CommandEventData eventData, InterceptionResult<object> result,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        await SetTenantContextAsync(command, cancellationToken).ConfigureAwait(false);
        return result;
    }

    private async Task SetTenantContextAsync(DbCommand command, CancellationToken ct)
    {
        var setCmd = command.Connection!.CreateCommand();
        await using (setCmd.ConfigureAwait(false))
        {
            setCmd.Transaction = command.Transaction;

        if (tenant.IsolationMode == IsolationMode.Isolated)
        {
            setCmd.CommandText = "SELECT set_config('search_path', @path, true)";
            setCmd.Parameters.Add(new NpgsqlParameter("path", $"{tenant.SchemaName}, public"));
        }
        else
        {
            setCmd.CommandText = "SELECT set_config('app.current_tenant_id', @tid, true)";
            setCmd.Parameters.Add(new NpgsqlParameter("tid", tenant.TenantId.ToString()));
        }

        await setCmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
        }
    }
}
