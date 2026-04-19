// src/Cells/Xinfer/AHS.Cell.Xinfer.Infrastructure/Services/OutboxPublisherService.cs
using System;
using System.Data;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AHS.Cell.Xinfer.Contracts;
using AHS.Cell.Xinfer.Application.Persistence.Entities;
using AHS.Common.Contracts;
using AHS.Common.Domain;
using AHS.Common.Infrastructure.Persistence;
using Dapper;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AHS.Cell.Xinfer.Infrastructure.Services;

public class OutboxPublisherService(
    IDbConnectionFactory connectionFactory,
    ICellEventPublisher  eventPublisher,
    ILogger<OutboxPublisherService> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        logger.LogInformation("Xinfer Outbox Publisher started.");

        while (!ct.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingMessagesAsync(ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing outbox messages.");
            }

            await Task.Delay(TimeSpan.FromSeconds(5), ct);
        }
    }

    private async Task ProcessPendingMessagesAsync(CancellationToken ct)
    {
        using var connection = await connectionFactory.CreateAsync(ct);
        if (connection.State != ConnectionState.Open)
        {
             // connection.Open(); // Some factories might open it, others might not. 
             // Common pattern in AHS seems to be Async creation.
        }

        const string selectSql = @"
            SELECT id, event_type as EventType, payload_json as PayloadJson 
            FROM outbox_messages 
            WHERE processed_at IS NULL 
            LIMIT 20";

        var messages = await connection.QueryAsync<OutboxMessage>(selectSql);

        foreach (var msg in messages)
        {
            try
            {
                var type = GetEventType(msg.EventType);
                if (type == null) throw new InvalidOperationException($"Unknown event type: {msg.EventType}");

                var evt = (ICellEvent)JsonSerializer.Deserialize(msg.PayloadJson, type, XinferContractsJsonContext.Default)!;
                
                await eventPublisher.PublishAsync(evt, ct);

                const string updateSql = "UPDATE outbox_messages SET processed_at = @Now WHERE id = @Id";
                await connection.ExecuteAsync(updateSql, new { Now = DateTimeOffset.UtcNow, msg.Id });
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to publish outbox message {MessageId}", msg.Id);
                const string errorSql = "UPDATE outbox_messages SET error = @Error WHERE id = @Id";
                await connection.ExecuteAsync(errorSql, new { Error = ex.Message, msg.Id });
            }
        }
    }

    private static Type? GetEventType(string typeName) => typeName switch
    {
        nameof(PredictOkEvent)       => typeof(PredictOkEvent),
        nameof(ReadinessFailEvent)  => typeof(ReadinessFailEvent),
        nameof(RetrainRequiredEvent) => typeof(RetrainRequiredEvent),
        _ => null
    };
}
