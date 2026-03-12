namespace AHS.SharedKernel;

/// <summary>Marker interface for DDD aggregate roots. The <see cref="IsAggregateRoot"/> property satisfies CA1040.</summary>
public interface IAggregateRoot
{
    /// <summary>Gets a value indicating whether this entity is an aggregate root. Always returns <see langword="true"/>.</summary>
    static virtual bool IsAggregateRoot => true;
}