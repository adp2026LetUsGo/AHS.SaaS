using AHS.Common.Domain;
using AHS.Cell.Xinfer.Domain.Events;
using AHS.Cell.Xinfer.Domain.ValueObjects;
using AHS.Cell.Xinfer.Domain.Exceptions;

namespace AHS.Cell.Xinfer.Domain.Aggregates;

public class ShipmentProfile : AggregateRoot
{
    public ShipmentIdentity Identity { get; private set; }
    public CarrierProfile Carrier { get; private set; }
    public string Status { get; private set; } = "Created";

    // Sequence progress tracking
    private bool _interpreted;
    private bool _readinessValidated;
    private bool _divergenceDetected;
    private bool _historicalSelected;
    private bool _retrainDecided;
    private bool _retrained;
    private bool _predictionCompleted;
    private bool _recommendationsGenerated;

    private ShipmentProfile() { }

    public static ShipmentProfile Create(
        Guid id,
        Guid tenantId,
        ShipmentIdentity identity,
        CarrierProfile carrier)
    {
        var s = new ShipmentProfile();
        s.Apply(new ShipmentProfileCreated(id, tenantId, identity, carrier));
        return s;
    }

    public static new ShipmentProfile Rehydrate(IEnumerable<DomainEvent> history)
    {
        var s = new ShipmentProfile();
        s.InternalRehydrate(history);
        return s;
    }

    private void InternalRehydrate(IEnumerable<DomainEvent> history)
    {
        // Calling base.Rehydrate which calls When
        base.Rehydrate(history);
    }

    // Step 2: Readiness
    public void RecordReadiness(string status, string[] errors, string[] warnings)
    {
        // Implicitly interpreted if we are here
        _interpreted = true; 
        
        Apply(new ReadinessValidated(Id, status, errors, warnings));
    }

    // Step 3: Divergence
    public void RecordDivergence(bool hasDivergence, string[] divergences)
    {
        if (!_readinessValidated)
            throw new XinferSequenceViolationException("Readiness validation must be completed before divergence detection.");
            
        Apply(new DivergenceDetected(Id, hasDivergence, divergences));
    }

    // Step 4: Historical Selection
    public void RecordHistoricalSelection(string season, int recordCount, string? includedCarrier)
    {
        if (!_divergenceDetected)
            throw new XinferSequenceViolationException("Divergence detection must be completed before historical selection.");
            
        Apply(new HistoricalDatasetSelected(Id, season, recordCount, includedCarrier));
    }

    // Step 5: Retrain Decision
    public void RecordRetrainDecision(bool shouldRetrain, string reason, string severity)
    {
        if (!_historicalSelected)
            throw new XinferSequenceViolationException("Historical selection must be completed before retrain decision.");
            
        Apply(new RetrainDecisionMade(Id, shouldRetrain, reason, severity));
    }

    // Step 6: Retraining (Optional)
    public void RecordRetraining(Guid modelVersionId, int versionNumber)
    {
        if (!_retrainDecided)
            throw new XinferSequenceViolationException("Retrain decision must be made before retraining.");
            
        Apply(new ModelRetrained(Id, modelVersionId, versionNumber));
    }

    // Step 7: Prediction
    public void RecordPrediction(Guid predictionId, double riskScore, string riskLevel, double pessimisticTtf, XaiDna dna)
    {
        // RULE: Prediction MUST NEVER execute before Data Readiness returns Acceptable.
        if (Status != "Acceptable" && Status != "Risky")
            throw new XinferSequenceViolationException($"Prediction blocked: Readiness status is '{Status}'.");

        if (!_retrainDecided)
            throw new XinferSequenceViolationException("Retrain decision must be made before prediction.");

        Apply(new PredictionCompleted(Id, predictionId, riskScore, riskLevel, pessimisticTtf, dna));
    }

    // Step 8: Recommendations
    public void RecordRecommendations(int count)
    {
        if (!_predictionCompleted)
            throw new XinferSequenceViolationException("Prediction must be completed before generating recommendations.");
            
        Apply(new RecommendationsGenerated(Id, count));
    }

    public bool IsSequenceComplete() => 
        _interpreted && 
        _readinessValidated && 
        _divergenceDetected && 
        _historicalSelected && 
        _retrainDecided && 
        _retrained && 
        _predictionCompleted && 
        _recommendationsGenerated;

    protected override void When(DomainEvent evt)
    {
        switch (evt)
        {
            case ShipmentProfileCreated e:
                Id = e.ShipmentId;
                TenantId = e.TenantId;
                Identity = e.Identity;
                Carrier = e.Carrier;
                _interpreted = true;
                break;

            case ReadinessValidated e:
                Status = e.Status;
                _readinessValidated = true;
                break;

            case DivergenceDetected:
                _divergenceDetected = true;
                break;

            case HistoricalDatasetSelected:
                _historicalSelected = true;
                break;

            case RetrainDecisionMade e:
                _retrainDecided = true;
                // If we don't need retraining, we skip step 6
                if (!e.ShouldRetrain) _retrained = true; 
                break;

            case ModelRetrained:
                _retrained = true;
                break;

            case PredictionCompleted:
                _predictionCompleted = true;
                break;

            case RecommendationsGenerated:
                _recommendationsGenerated = true;
                break;
        }
    }
}
