// src/Cells/Xinfer/AHS.Cell.Xinfer.Infrastructure/Engines/OnnxInferenceEngine.cs
// Inference Bridge — ONNX engine stub. Activates when OnnxModelPath is configured.
// ADR-010: swap MockInferenceEngine for this via DI config; no application logic changes.
using AHS.Cell.Xinfer.Application.Ports;
using AHS.Cell.Xinfer.Application.Contracts;
using AHS.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace AHS.Cell.Xinfer.Infrastructure.Engines;

/// <summary>
/// ONNX Runtime inference engine. Loads a compiled .onnx model from the path
/// declared in <c>Xinfer:OnnxModelPath</c> (appsettings.json).
///
/// <para><b>Activation checklist (Ruta B):</b></para>
/// <list type="number">
///   <item>Set <c>Xinfer:OnnxModelPath</c> to the absolute path of your .onnx file.</item>
///   <item>Replace <c>IInferenceEngine → MockInferenceEngine</c> registration with
///         <c>IInferenceEngine → OnnxInferenceEngine</c> in XinferServiceExtensions.</item>
///   <item>Implement <see cref="RunAsync"/> using <c>Microsoft.ML.OnnxRuntime.InferenceSession</c>.</item>
/// </list>
///
/// <para><b>AOT Note:</b> Microsoft.ML.OnnxRuntime ≥ 1.19 ships with AOT-compatible
/// metadata. No reflection is used in the stub; the full implementation must
/// also avoid dynamic IL (use typed tensor accessors only).</para>
/// </summary>
public sealed class OnnxInferenceEngine : IInferenceEngine
{
    public string EngineId => "onnx-stub-v1";

    private readonly ILogger<OnnxInferenceEngine> _logger;
    private readonly string? _modelPath;

    public OnnxInferenceEngine(
        ILogger<OnnxInferenceEngine> logger,
        IConfiguration config)
    {
        _logger    = logger;
        _modelPath = config["Xinfer:OnnxModelPath"];
    }

    /// <inheritdoc />
    /// <exception cref="NotSupportedException">
    /// Always thrown — ONNX engine requires a loaded model. Configure
    /// <c>Xinfer:OnnxModelPath</c> in appsettings.json and implement this method
    /// using <c>Microsoft.ML.OnnxRuntime.InferenceSession</c>.
    /// </exception>
    public Task<Result<InferenceOutput_v1>> RunAsync(InferenceInput_v1 input, CancellationToken ct = default)
    {
        _logger.LogWarning(
            "[Inference Bridge] OnnxInferenceEngine invoked but no model is loaded. " +
            "ModelPath={ModelPath}. Configure Xinfer:OnnxModelPath to activate Ruta B.",
            _modelPath ?? "<not configured>");

        // ── TODO (Ruta B): Replace this throw with the ONNX session implementation:
        //
        //   using var session = new InferenceSession(_modelPath!);
        //   var inputTensor  = BuildOnnxTensor(input);
        //   var results      = session.Run([NamedOnnxValue.CreateFromTensor("input", inputTensor)]);
        //   return Task.FromResult(Result.Success(MapResults(results)));

        throw new NotSupportedException(
            "OnnxInferenceEngine requires a loaded .onnx model. " +
            "Set 'Xinfer:OnnxModelPath' in appsettings.json and implement RunAsync for Ruta B.");
    }
}
