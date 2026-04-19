// src/Cells/Xinfer/AHS.Cell.Xinfer.Infrastructure/Services/XinferHealthService.cs
using AHS.Cell.Xinfer.Application.DTOs;
using AHS.Cell.Xinfer.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace AHS.Cell.Xinfer.Infrastructure.Services;

public class XinferHealthService(XinferDbContext db)
{
    public async Task<XinferHealthDto> GetOperationalHealthAsync(CancellationToken ct)
    {
        // 1. Check Model Lifecycle State
        var activeModel = await db.Models.FirstOrDefaultAsync(m => m.IsActive, ct);
        var state = activeModel switch
        {
            null => XinferLifecycleState.Degraded, // No model
            _ when activeModel.AccuracyScore < 0.80 => XinferLifecycleState.RetrainingRequired,
            _ => XinferLifecycleState.Operational
        };

        // 2. Check Outbox Health
        // Ignore the query filter to count ALL pending messages (though filter is processed_at == null)
        var pendingMessages = await db.OutboxMessages.CountAsync(ct);

        return new XinferHealthDto 
        {
             CellState = state,
             ActiveModelVersion = activeModel?.VersionNumber ?? 0,
             LastRetrainAt = activeModel?.TrainedAt ?? DateTimeOffset.MinValue,
             PendingOutboxCount = pendingMessages,
             Healthy = state == XinferLifecycleState.Operational && pendingMessages < 100
        };
    }
}
