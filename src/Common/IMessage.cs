using System.Threading.Tasks;

namespace AHS.Common;

/// <summary>Marker interface for CQRS requests.</summary>
public interface IRequest<out TResponse>
{
    /// <summary>Gets a value indicating whether this is a valid request marker. Always returns true.</summary>
    static virtual bool RequestMarker => true;
}

public interface IRequestHandler<in TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request);
}