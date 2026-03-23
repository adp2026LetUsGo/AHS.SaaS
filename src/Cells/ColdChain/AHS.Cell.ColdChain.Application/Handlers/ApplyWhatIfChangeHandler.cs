// src/Cells/ColdChain/AHS.Cell.ColdChain.Application/Handlers/ApplyWhatIfChangeHandler.cs
using AHS.Cell.ColdChain.Application.Commands;
using AHS.Cell.ColdChain.Domain.Ports;
using AHS.Cell.ColdChain.Domain.Events;

namespace AHS.Cell.ColdChain.Application.Handlers;

public sealed class ApplyWhatIfChangeHandler(
    IShipmentRepository repository)
{
    public async Task HandleAsync(ApplyWhatIfChangeCommand cmd, CancellationToken ct)
    {
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

        await repository.AppendAsync(cmd.ShipmentId, [evt], -1, ct);
    }
}
