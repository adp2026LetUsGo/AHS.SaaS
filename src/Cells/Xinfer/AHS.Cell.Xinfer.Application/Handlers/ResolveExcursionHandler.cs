// src/Cells/Xinfer/AHS.Cell.Xinfer.Application/Handlers/ResolveExcursionHandler.cs
using AHS.Cell.Xinfer.Application.Commands;
using AHS.Cell.Xinfer.Domain.Ports;

namespace AHS.Cell.Xinfer.Application.Handlers;

public sealed class ResolveExcursionHandler(
    IShipmentRepository repository)
{
    public async Task HandleAsync(ResolveExcursionCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);
        var shipment = await repository.LoadAsync(cmd.ShipmentId, ct).ConfigureAwait(false);
        
        shipment.ResolveExcursion(
            cmd.ExcursionEventId,
            cmd.ResolutionNote,
            cmd.SignedById,
            cmd.SignedByName,
            cmd.ReasonForChange);

        await repository.AppendAsync(shipment.Id, shipment.UncommittedEvents, -1, ct).ConfigureAwait(false);
    }
}
