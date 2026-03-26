// src/Cells/Xinfer/AHS.Cell.Xinfer.Infrastructure/ServiceBus/XinferCellEventPublisher.cs
using AHS.Common.Domain;
using AHS.Cell.Xinfer.Contracts;
using AHS.Common.Contracts;

namespace AHS.Cell.Xinfer.Infrastructure.ServiceBus;

public sealed class XinferCellEventPublisher : ICellEventPublisher
{
    public Task PublishAsync(ICellEvent evt, CancellationToken ct)
    {
        // Integration with Azure Service Bus / RabbitMQ would go here.
        // For now, satisfies the contract for inter-cell communication.
        return Task.CompletedTask;
    }
}
// Note: ICellEvent is defined in AHS.Common.Contracts
