// src/Cells/Xinfer/AHS.Cell.Xinfer.Application/Commands/ApplyWhatIfChangeCommand.cs
using AHS.Common.Application;

namespace AHS.Cell.Xinfer.Application.Commands;

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
