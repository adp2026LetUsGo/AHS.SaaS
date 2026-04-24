// src/Cells/Xinfer/AHS.Cell.Xinfer.Infrastructure/Extensions/XinferServiceExtensions.cs
using AHS.Cell.Xinfer.Application.Ports;
using AHS.Cell.Xinfer.Application.Handlers;
using AHS.Cell.Xinfer.Application.Queries;
using AHS.Cell.Xinfer.Application.Oracle;
using Microsoft.EntityFrameworkCore;
using AHS.Common.Domain;
using AHS.Cell.Xinfer.Infrastructure.ServiceBus;
using AHS.Cell.Xinfer.Infrastructure.Persistence;
using AHS.Cell.Xinfer.Domain.Ports;
using AHS.Common.Infrastructure.Tenancy;
using AHS.Common.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using AHS.Cell.Xinfer.Infrastructure.Services;
using AHS.Cell.Xinfer.Infrastructure.Engines;

namespace AHS.Cell.Xinfer.Infrastructure.Extensions;

public static class XinferServiceExtensions
{
    public static IServiceCollection AddXinferInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("DefaultConnection");

        services.AddDbContext<XinferDbContext>((sp, options) => {
            options.UseNpgsql(connectionString);
            options.AddInterceptors(sp.GetRequiredService<TenantSessionInterceptor>());
        });

        services.AddScoped<IShipmentRepository, ShipmentRepository>();
        services.AddScoped<IShipmentReadRepository, ShipmentReadRepository>();
        services.AddSingleton<LogisticsOracle>();
        services.AddSingleton<ICellEventPublisher, XinferCellEventPublisher>();
        // Inference Bridge — swap MockInferenceEngine → OnnxInferenceEngine for Ruta B
        services.AddScoped<IInferenceEngine, MockInferenceEngine>();
        services.AddScoped<IDataReadinessEngine, MockReadinessEngine>();
        services.AddScoped<IInferenceService, MockInferenceService>();
        services.AddScoped<XinferHealthService>();

        // Explicit Handler Injection (No MediatR per Prompt Constraints)
        services.AddScoped<RegisterShipmentHandler>();
        services.AddScoped<RecordExcursionHandler>();
        services.AddScoped<ResolveExcursionHandler>();
        services.AddScoped<SealShipmentHandler>();
        services.AddScoped<ApplyWhatIfChangeHandler>();

        // Query objects — must be registered for DI resolution in Slim builder endpoints
        services.AddScoped<ListActiveShipmentsQuery>();
        services.AddScoped<GetShipmentByIdQuery>();
        services.AddScoped<GetXinferReportQuery>();

        services.AddScoped<IThermalDataSource, MockThermalDataSource>();

        return services;
    }
}
