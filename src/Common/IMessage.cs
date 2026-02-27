namespace AHS.Common;

/// <summary>Marker interface for CQRS requests. The <see cref="RequestMarker"/> property satisfies CA1040.</summary>
public interface IRequest<out TResponse>
{
    /// <summary>Gets a value indicating whether this is a valid request marker. Always returns <see langword="true"/>.</summary>
    static virtual bool RequestMarker => true;
}

public interface IRequestHandler<in TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request);
}
