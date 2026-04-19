// src/Cells/Xinfer/AHS.Cell.Xinfer.Application/Ports/IXinferDbContext.cs
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using AHS.Cell.Xinfer.Application.Persistence.Entities;
using AHS.Cell.Xinfer.Domain.Aggregates;

namespace AHS.Cell.Xinfer.Application.Ports;

public interface IXinferDbContext
{
    DbSet<OutboxMessage> OutboxMessages { get; }
    DbSet<ModelVersion> Models { get; }
    
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default);
}
