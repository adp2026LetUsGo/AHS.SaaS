// src/Cells/ColdChain/AHS.Cell.ColdChain.Infrastructure/ServiceBus/ColdChainCellEventPublisher.cs
using AHS.Common.Domain;
using AHS.Cell.ColdChain.Contracts;
using AHS.Common.Contracts;

namespace AHS.Cell.ColdChain.Infrastructure.ServiceBus;

public sealed class ColdChainCellEventPublisher : ICellEventPublisher
{
    public Task PublishAsync(ICellEvent @event, CancellationToken ct)
    {
        // Integration with Azure Service Bus / RabbitMQ would go here.
        // For now, satisfies the contract for inter-cell communication.
        return Task.CompletedTask;
    }
}
// Note: ICellEvent is defined in AHS.Common.Contracts
