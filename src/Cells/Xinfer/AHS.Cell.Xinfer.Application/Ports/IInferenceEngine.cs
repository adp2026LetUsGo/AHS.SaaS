// src/Cells/Xinfer/AHS.Cell.Xinfer.Application/Ports/IInferenceEngine.cs
// Inference Bridge — decouples the Cell from any concrete AI/ML engine.
// ADR-010: swap MockInferenceEngine ↔ OnnxInferenceEngine via config; Cell logic unchanged.
using System.Threading;
using System.Threading.Tasks;
using AHS.Cell.Xinfer.Application.Contracts;
using AHS.Common;

namespace AHS.Cell.Xinfer.Application.Ports;

/// <summary>
/// Inference Bridge port. Implementations must be stateless and AOT-safe.
/// IInferenceService delegates to this engine; the Cell Application layer
/// never depends on a concrete engine directly.
/// </summary>
public interface IInferenceEngine
{
    /// <summary>
    /// Runs the inference pipeline on the given input and returns a scored result.
    /// Implementations must emit a deterministic <see cref="InferenceOutput_v1"/>
    /// including a <c>confidence_score</c> in the range [0.0, 1.0].
    /// </summary>
    Task<Result<InferenceOutput_v1>> RunAsync(InferenceInput_v1 input, CancellationToken ct = default);

    /// <summary>
    /// Human-readable identifier of this engine (e.g., "mock-v1", "onnx-v2.1").
    /// Used in logging and the Dev Corner overlay.
    /// </summary>
    string EngineId { get; }
}
