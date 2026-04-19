namespace AHS.Cell.Xinfer.Domain.ValueObjects;

public record RetrainDecision(
    bool ShouldRetrain,
    string Reason,
    string Severity // "Low" | "Medium" | "High" | "Critical"
);
