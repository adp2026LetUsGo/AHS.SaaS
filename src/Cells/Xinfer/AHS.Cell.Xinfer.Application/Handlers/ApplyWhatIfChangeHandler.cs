// src/Cells/Xinfer/AHS.Cell.Xinfer.Application/Handlers/ApplyWhatIfChangeHandler.cs
using AHS.Cell.Xinfer.Application.Commands;
using AHS.Cell.Xinfer.Domain.Ports;
using AHS.Cell.Xinfer.Domain.Events;

namespace AHS.Cell.Xinfer.Application.Handlers;

public sealed class ApplyWhatIfChangeHandler(
    IShipmentRepository repository)
{
    public async Task HandleAsync(ApplyWhatIfChangeCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);
        // What-If changes are events appended directly to the stream (GxP requirement)
        var evt = new WhatIfParameterChanged(
            cmd.ShipmentId,
            cmd.ParameterName,
            cmd.PreviousValue,
            cmd.NewValue,
            cmd.ReasonForChange,
            cmd.SignedById,
            cmd.SignedByName,
            DateTimeOffset.UtcNow,
            cmd.TenantId);

        await repository.AppendAsync(cmd.ShipmentId, [evt], -1, ct).ConfigureAwait(false);
    }
}
