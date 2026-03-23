// src/Cells/ColdChain/AHS.Cell.ColdChain.Application/Commands/ResolveExcursionCommand.cs
using AHS.Common.Application;

namespace AHS.Cell.ColdChain.Application.Commands;

public record ResolveExcursionCommand(
    Guid           ShipmentId,
    Guid           ExcursionEventId,
    string         ResolutionNote,
    Guid           TenantId,
    Guid           SignedById,
    string         SignedByName,
    string         ReasonForChange
) : SignedCommand(SignedById, SignedByName, ReasonForChange);
