namespace AHS.Platform.Compliance;

public class AuditTrailService(IAuditRepository repository)
{
    private readonly IAuditRepository _repository = repository;

    public async Task SaveAsync(AuditRecord record) => await _repository.SaveAsync(record);
}
