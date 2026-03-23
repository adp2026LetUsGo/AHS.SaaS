// src/Foundation/AHS.Common/Infrastructure/Persistence/IDbConnectionFactory.cs
using System.Data;

namespace AHS.Common.Infrastructure.Persistence;

public interface IDbConnectionFactory
{
    Task<IDbConnection> CreateAsync(CancellationToken ct);
}
