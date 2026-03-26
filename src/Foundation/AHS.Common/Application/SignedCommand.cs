namespace AHS.Common.Application;

public abstract record SignedCommand
{
    private readonly string _reasonForChange = "";
    public string ReasonForChange 
    { 
        get => _reasonForChange; 
        init 
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ElectronicSignatureRequiredException(
                    "ReasonForChange is mandatory for all write operations — Blueprint V3.1.1 G4.");
            _reasonForChange = value;
        }
    }

    public Guid   SignedById      { get; init; }
    public string SignedByName    { get; init; } = string.Empty;
    public DateTimeOffset  SignedAt        { get; init; } = DateTimeOffset.UtcNow;

    protected SignedCommand(Guid signedById, string signedByName, string reasonForChange)
    {
        SignedById = signedById;
        SignedByName = signedByName;
        ReasonForChange = reasonForChange;
    }

    protected SignedCommand() { }
}

public sealed class ElectronicSignatureRequiredException : Exception
{
    public ElectronicSignatureRequiredException() { }
    public ElectronicSignatureRequiredException(string message) : base(message) { }
    public ElectronicSignatureRequiredException(string message, Exception innerException) : base(message, innerException) { }
}
