// src/Foundation/AHS.Common/Contracts/ICellEventPublisher.cs
namespace AHS.Common.Contracts;

public interface ICellEventPublisher
{
    Task PublishAsync(ICellEvent evt, CancellationToken ct);
}
