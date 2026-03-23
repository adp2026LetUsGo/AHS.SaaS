// tests/Cells/ColdChain/AHS.Cell.ColdChain.Tests/Integration/ColdChainWebAppFactory.cs
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using AHS.Cell.ColdChain.API;
using AHS.Cell.ColdChain.Infrastructure.Persistence;
using AHS.Common.Infrastructure.Tenancy;
using Testcontainers.PostgreSql;
using Xunit;

namespace AHS.Cell.ColdChain.Tests.Integration;

public class ColdChainWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithDatabase("coldchain")
        .WithUsername("ahs")
        .WithPassword("ahs")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ColdChainDbContext>>();
            services.AddDbContext<ColdChainDbContext>((sp, options) => {
                options.UseNpgsql(_dbContainer.GetConnectionString());
                options.AddInterceptors(sp.GetRequiredService<TenantSessionInterceptor>());
            });
        });
    }

    public async Task InitializeAsync() 
    {
        await _dbContainer.StartAsync();
        
        // Apply RLS Policy for Integration Testing
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ColdChainDbContext>();
        await context.Database.EnsureCreatedAsync();
        
        const string sql = @"
            ALTER TABLE shipments ENABLE ROW LEVEL SECURITY;
            DROP POLICY IF EXISTS tenant_isolation_policy ON shipments;
            CREATE POLICY tenant_isolation_policy ON shipments
                USING (tenant_id = current_setting('app.current_tenant_id', true)::uuid);";
        
        await context.Database.ExecuteSqlRawAsync(sql);
    }
    public new Task DisposeAsync() => _dbContainer.StopAsync();
}
