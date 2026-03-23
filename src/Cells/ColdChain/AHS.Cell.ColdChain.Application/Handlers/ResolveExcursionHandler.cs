// src/Cells/ColdChain/AHS.Cell.ColdChain.Application/Handlers/ResolveExcursionHandler.cs
using AHS.Cell.ColdChain.Application.Commands;
using AHS.Cell.ColdChain.Domain.Ports;

namespace AHS.Cell.ColdChain.Application.Handlers;

public sealed class ResolveExcursionHandler(
    IShipmentRepository repository)
{
    public async Task HandleAsync(ResolveExcursionCommand cmd, CancellationToken ct)
    {
        var shipment = await repository.LoadAsync(cmd.ShipmentId, ct);
        
        shipment.ResolveExcursion(
            cmd.ExcursionEventId,
            cmd.ResolutionNote,
            cmd.SignedById,
            cmd.SignedByName,
            cmd.ReasonForChange);

        await repository.AppendAsync(shipment.Id, shipment.UncommittedEvents, -1, ct);
    }
}
