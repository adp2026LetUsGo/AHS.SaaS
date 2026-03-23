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
    public string SignedByName    { get; init; }
    public DateTimeOffset  SignedAt        { get; init; } = DateTimeOffset.UtcNow;

    protected SignedCommand(Guid signedById, string signedByName, string reasonForChange)
    {
        SignedById = signedById;
        SignedByName = signedByName;
        ReasonForChange = reasonForChange;
    }

    protected SignedCommand() { }
}

public sealed class ElectronicSignatureRequiredException(string message)
    : Exception(message);
