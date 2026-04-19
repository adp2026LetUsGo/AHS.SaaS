// src/Cells/Xinfer/AHS.Cell.Xinfer.Application/Handlers/SubmitShipmentHandler.cs
using AHS.Cell.Xinfer.Application.Commands;
using AHS.Cell.Xinfer.Domain.Aggregates;
using AHS.Cell.Xinfer.Domain.Ports;
using AHS.Cell.Xinfer.Application.Ports;
using AHS.Cell.Xinfer.Application.Persistence.Entities;
using AHS.Cell.Xinfer.Contracts;
using AHS.Common.Domain;
using AHS.Common.Contracts;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using AHS.Cell.Xinfer.Domain.ValueObjects;
using AHS.Cell.Xinfer.Domain.DomainServices;

namespace AHS.Cell.Xinfer.Application.Handlers;

public class SubmitShipmentHandler(
    IShipmentRepository  repository,
    IReadinessValidator  readinessValidator,
    IHistoricalSelector historicalSelector,
    IDivergenceDetector  divergenceDetector,
    IPredictionEngine    predictionEngine,
    IRetrainDecider      retrainDecider,
    IXinferDbContext      _db
)
{
    public async Task HandleAsync(SubmitShipmentCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        using var transaction = await _db.BeginTransactionAsync(ct);
        try 
        {
            var shipment = await repository.LoadAsync(command.ShipmentId, ct).ConfigureAwait(false);
            
            var identity = new ShipmentIdentity(
                "Unknown", 
                command.CargoType.ToString(), 
                command.PackagingType, 
                command.RouteId, 
                command.PlannedDeparture);

            var carrier = new CarrierProfile(
                command.CarrierId, 
                command.CarrierReliabilityScore, 
                command.CarrierIncidents12M);

            var readiness = await readinessValidator.ValidateAsync(
                identity, carrier, command.ForecastMaxCelsius, command.ForecastMinCelsius, 
                command.ForecastHumidityPct, command.EstimatedDurationHours, 
                command.PlannedDeparture.Hour, ct).ConfigureAwait(false);

            if (readiness.Status == "NotAcceptable")
            {
                var failEvent = new ReadinessFailEvent
                {
                    TenantSlug = command.TenantSlug,
                    ShipmentId = command.ShipmentId,
                    Reason = string.Join("; ", readiness.Errors)
                };
                await WriteToOutboxAsync(failEvent, ct);
                await _db.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
                return;
            }

            var dataset = await historicalSelector.SelectAsync(identity, carrier, true, ct).ConfigureAwait(false);
            
            var divergence = await divergenceDetector.DetectAsync(
                identity, carrier, command.ForecastMaxCelsius, command.ForecastMinCelsius, 
                command.ForecastHumidityPct, command.EstimatedDurationHours, 
                command.PlannedDeparture.Hour, ct).ConfigureAwait(false);

            var retrain = await retrainDecider.EvaluateAsync(dataset, divergence, ct).ConfigureAwait(false);

            if (retrain.ShouldRetrain)
            {
                var retrainEvent = new RetrainRequiredEvent
                {
                    TenantSlug = command.TenantSlug,
                    Reason = retrain.Reason,
                    Severity = retrain.Severity
                };
                await WriteToOutboxAsync(retrainEvent, ct);
            }

            var activeModel = await _db.Models.FirstOrDefaultAsync(m => m.IsActive, ct);
            var prediction = await predictionEngine.PredictAsync(identity, carrier, dataset, activeModel, ct).ConfigureAwait(false);

            shipment.RecordPrediction(prediction.RiskScore, prediction.AccuracyScore, prediction.ReliabilityScore);
            await repository.AppendAsync(shipment.Id, shipment.UncommittedEvents, -1, ct).ConfigureAwait(false);

            var okEvent = new PredictOkEvent
            {
                TenantSlug = command.TenantSlug,
                ShipmentId = shipment.Id,
                RiskScore = prediction.RiskScore,
                AccuracyScore = prediction.AccuracyScore,
                ReliabilityScore = prediction.ReliabilityScore
            };
            await WriteToOutboxAsync(okEvent, ct);

            await _db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    private async Task WriteToOutboxAsync<T>(T evt, CancellationToken ct) where T : ICellEvent
    {
        var outboxMessage = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            EventType = typeof(T).Name,
            PayloadJson = JsonSerializer.Serialize(evt, XinferContractsJsonContext.Default.GetTypeInfo(typeof(T))!),
            OccurredAt = DateTimeOffset.UtcNow
        };
        await _db.OutboxMessages.AddAsync(outboxMessage, ct);
    }
}
