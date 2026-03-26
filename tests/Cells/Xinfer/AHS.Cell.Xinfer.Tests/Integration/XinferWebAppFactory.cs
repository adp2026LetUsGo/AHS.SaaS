// tests/Cells/Xinfer/AHS.Cell.Xinfer.Tests/Integration/XinferWebAppFactory.cs
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using AHS.Cell.Xinfer.API;
using AHS.Cell.Xinfer.Infrastructure.Persistence;
using AHS.Common.Infrastructure.Tenancy;
using Testcontainers.PostgreSql;
using Xunit;

namespace AHS.Cell.Xinfer.Tests.Integration;

#pragma warning disable CA1515
public sealed class XinferWebAppFactory : WebApplicationFactory<global::Program>, IAsyncLifetime
#pragma warning restore CA1515
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:16.2")
        .WithDatabase("Xinfer")
        .WithUsername("ahs")
        .WithPassword("ahs")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<XinferDbContext>>();
            services.AddDbContext<XinferDbContext>((sp, options) => {
                options.UseNpgsql(_dbContainer.GetConnectionString());
                options.AddInterceptors(sp.GetRequiredService<TenantSessionInterceptor>());
            });
        });
    }

    public async Task InitializeAsync() 
    {
        await _dbContainer.StartAsync().ConfigureAwait(false);
        
        // Apply RLS Policy for Integration Testing
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<XinferDbContext>();
        await context.Database.EnsureCreatedAsync().ConfigureAwait(false);
        
        const string sql = @"
            ALTER TABLE shipments ENABLE ROW LEVEL SECURITY;
            DROP POLICY IF EXISTS tenant_isolation_policy ON shipments;
            CREATE POLICY tenant_isolation_policy ON shipments
                USING (tenant_id = current_setting('app.current_tenant_id', true)::uuid);";
        
        await context.Database.ExecuteSqlRawAsync(sql).ConfigureAwait(false);
    }
    public new Task DisposeAsync() => _dbContainer.StopAsync();

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _dbContainer.DisposeAsync().AsTask().GetAwaiter().GetResult();
        }
        base.Dispose(disposing);
    }
}
