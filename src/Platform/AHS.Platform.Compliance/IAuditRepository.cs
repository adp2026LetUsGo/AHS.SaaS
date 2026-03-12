using AHS.Common;
namespace AHS.Platform.Compliance; public interface IAuditRepository { Task SaveAsync(AuditRecord record); Task<Result<string>> CheckHealthAsync(); }