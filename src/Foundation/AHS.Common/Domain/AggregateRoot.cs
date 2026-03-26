// src/Foundation/AHS.Common/Domain/AggregateRoot.cs
namespace AHS.Common.Domain;

public abstract class AggregateRoot
{
    public Guid Id       { get; protected set; }
    public Guid TenantId { get; protected set; }
    public int  Version  { get; private set; }

    private readonly List<DomainEvent> _uncommitted = [];
    public IReadOnlyList<DomainEvent> UncommittedEvents => _uncommitted;

    protected void Apply(DomainEvent evt) { When(evt); _uncommitted.Add(evt); Version++; }

    public void Rehydrate(IEnumerable<DomainEvent> history)
    {
        ArgumentNullException.ThrowIfNull(history);
        foreach (var evt in history) { When(evt); Version++; }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Standard Event Sourcing nomenclature")]
    protected abstract void When(DomainEvent evt);
    public void ClearUncommitted() => _uncommitted.Clear();
}
