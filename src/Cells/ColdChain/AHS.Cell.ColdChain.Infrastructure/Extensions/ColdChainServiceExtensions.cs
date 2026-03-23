// src/Cells/ColdChain/AHS.Cell.ColdChain.Infrastructure/Extensions/ColdChainServiceExtensions.cs
using AHS.Cell.ColdChain.Application.Ports;
using AHS.Cell.ColdChain.Application.Handlers;
using AHS.Cell.ColdChain.Application.Oracle;
using Microsoft.EntityFrameworkCore;
using AHS.Common.Domain;
using AHS.Cell.ColdChain.Infrastructure.ServiceBus;
using AHS.Cell.ColdChain.Infrastructure.Persistence;
using AHS.Cell.ColdChain.Domain.Ports;
using AHS.Common.Infrastructure.Tenancy;
using AHS.Common.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace AHS.Cell.ColdChain.Infrastructure.Extensions;

public static class ColdChainServiceExtensions
{
    public static IServiceCollection AddColdChainInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("DefaultConnection");

        services.AddDbContext<ColdChainDbContext>((sp, options) => {
            options.UseNpgsql(connectionString);
            options.AddInterceptors(sp.GetRequiredService<TenantSessionInterceptor>());
        });

        services.AddScoped<IShipmentRepository, ShipmentRepository>();
        services.AddScoped<IShipmentReadRepository, ShipmentReadRepository>();
        services.AddSingleton<LogisticsOracle>();
        services.AddSingleton<ICellEventPublisher, ColdChainCellEventPublisher>();

        // Explicit Handler Injection (No MediatR per Prompt Constraints)
        services.AddScoped<RegisterShipmentHandler>();
        services.AddScoped<RecordExcursionHandler>();
        services.AddScoped<ResolveExcursionHandler>();
        services.AddScoped<SealShipmentHandler>();
        services.AddScoped<ApplyWhatIfChangeHandler>();

        return services;
    }
}
