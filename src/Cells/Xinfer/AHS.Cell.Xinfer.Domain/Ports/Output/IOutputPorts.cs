using AHS.Cell.Xinfer.Domain.Aggregates;
using AHS.Cell.Xinfer.Domain.Entities;
using AHS.Cell.Xinfer.Domain.ValueObjects;
using AHS.Cell.Xinfer.Domain.Events;
using AHS.Common.Domain;

namespace AHS.Cell.Xinfer.Domain.Ports.Output;

public interface IHistoricalRepository
{
    Task<IReadOnlyList<string>> GetKnownRoutesAsync(CancellationToken ct);
    Task<int> CountCompatibleAsync(string routeId, string packagingType, string season, CancellationToken ct);
    Task<bool> CarrierExistsAsync(string carrierId, CancellationToken ct);
    Task<double?> GetAvgRiskByRouteAsync(string routeId, CancellationToken ct);
    Task<double?> GetCarrierBaselineAsync(string carrierId, CancellationToken ct);
    Task<bool> HasCompatiblePackagingAsync(string routeId, string packagingType, CancellationToken ct);
}

public interface IModelRepository
{
    Task<ModelVersion?> GetActiveAsync(CancellationToken ct);
    Task<int> CountSinceLastRetrainAsync(CancellationToken ct);
}

public interface IXinferEventPublisher
{
    Task PublishAsync(DomainEvent evt, CancellationToken ct);
}
