// src/Cells/ColdChain/AHS.Cell.ColdChain.Application/Commands/ApplyWhatIfChangeCommand.cs
using AHS.Common.Application;

namespace AHS.Cell.ColdChain.Application.Commands;

public record ApplyWhatIfChangeCommand(
    Guid           ShipmentId,
    string         ParameterName,
    string         PreviousValue,
    string         NewValue,
    Guid           TenantId,
    Guid           SignedById,
    string         SignedByName,
    string         ReasonForChange
) : SignedCommand(SignedById, SignedByName, ReasonForChange);
