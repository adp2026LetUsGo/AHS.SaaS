using AHS.Common.Domain;
using AHS.Cell.Xinfer.Domain.Events;

namespace AHS.Cell.Xinfer.Domain.Aggregates;

public class ModelVersion : AggregateRoot
{
    public int VersionNumber { get; private set; }
    public DateTimeOffset TrainedAt { get; private set; }
    public int TrainingRecordsCount { get; private set; }
    public double AccuracyScore { get; private set; }
    public bool IsActive { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public byte[]? ModelBlob { get; private set; }

    private ModelVersion() { }

    public static ModelVersion Create(
        Guid id,
        Guid tenantId,
        int versionNumber,
        int trainingRecordsCount,
        double accuracyScore,
        string reason)
    {
        var m = new ModelVersion();
        m.Apply(new ModelVersionCreated(
            id, 
            tenantId, 
            versionNumber, 
            DateTimeOffset.UtcNow, 
            trainingRecordsCount, 
            accuracyScore, 
            reason));
        return m;
    }

    public static new ModelVersion Rehydrate(IEnumerable<DomainEvent> history)
    {
        var m = new ModelVersion();
        ((AggregateRoot)m).Rehydrate(history);
        return m;
    }

    public void Activate()
    {
        if (!IsActive)
            Apply(new ModelActivated(Id));
    }

    public void Deactivate()
    {
        if (IsActive)
            Apply(new ModelDeactivated(Id));
    }

    public void SetModelBlob(byte[] blob)
    {
        // Internal state change, not necessarily an event if it's large binary data
        // but for AOT we might need to handle it carefully.
        ModelBlob = blob;
    }

    protected override void When(DomainEvent evt)
    {
        switch (evt)
        {
            case ModelVersionCreated e:
                Id = e.ModelId;
                TenantId = e.TenantId;
                VersionNumber = e.VersionNumber;
                TrainedAt = e.TrainedAt;
                TrainingRecordsCount = e.TrainingRecordsCount;
                AccuracyScore = e.AccuracyScore;
                Reason = e.Reason;
                IsActive = false;
                break;

            case ModelActivated:
                IsActive = true;
                break;

            case ModelDeactivated:
                IsActive = false;
                break;
        }
    }
}
