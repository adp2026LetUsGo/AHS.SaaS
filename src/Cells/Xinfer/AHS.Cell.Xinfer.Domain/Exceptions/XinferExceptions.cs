namespace AHS.Cell.Xinfer.Domain.Exceptions;

public sealed class XinferSequenceViolationException : Exception
{
    public XinferSequenceViolationException(string message) : base(message) { }
}

public sealed class DataReadinessFailedException : Exception
{
    public DataReadinessFailedException(string message) : base(message) { }
}

public sealed class InsufficientHistoricalDataException : Exception
{
    public InsufficientHistoricalDataException(string message) : base(message) { }
}
