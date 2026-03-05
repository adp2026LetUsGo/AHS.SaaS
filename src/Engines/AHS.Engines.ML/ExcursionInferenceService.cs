using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System.Collections.Generic;
using System.Linq;

using System.Diagnostics.CodeAnalysis;

namespace AHS.Engines.ML;

[SuppressMessage("Design", "CA1515:Consider making public types internal", Justification = "ExcursionInferenceService is the primary API for ML inference across the AHS.SaaS monorepo, consumed by the GxP Traceability BC.")]
public sealed class ExcursionInferenceService : IDisposable
{
    private readonly InferenceSession _session;
    private bool _disposed;

    public ExcursionInferenceService(string modelPath)
    {
        if (!File.Exists(modelPath))
        {
            throw new FileNotFoundException($"❌ GxP CRITICAL ERROR: ONNX Model not found at [{Path.GetFullPath(modelPath)}]");
        }

        try
        {
            _session = new InferenceSession(modelPath);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"❌ GxP CRITICAL ERROR: Failed to load ONNX Model: {ex.Message}", ex);
        }
    }

    public InferenceResult PredictExcursion(float route, float carrier, float packaging, float weather)
    {
        var inputTensor = new DenseTensor<float>(new[] { 1, 4 });
        inputTensor[0, 0] = route;
        inputTensor[0, 1] = carrier;
        inputTensor[0, 2] = packaging;
        inputTensor[0, 3] = weather;

        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("float_input", inputTensor)
        };

        var outputNames = _session.OutputMetadata.Keys.ToList();
        var labelName = outputNames.Contains("output_label") ? "output_label" : outputNames[0];
        var probName = outputNames.Contains("output_probability") ? "output_probability" : outputNames[1];

        using var results = _session.Run(inputs);
        
        var labelResult = results.First(v => v.Name == labelName);
        var probResult = results.First(v => v.Name == probName);

        var label = labelResult.AsEnumerable<long>().First();
        float probability = 0.5f;

        // Extraction depends on zipmap setting (zipmap=False -> Tensor, zipmap=True -> IEnumerable<IDictionary>)
        if (probResult.Value is IEnumerable<IDictionary<long, float>> probMap)
        {
            probability = probMap.First()[(int)label];
        }
        else
        {
            var probTensor = probResult.AsTensor<float>();
            probability = probTensor[0, (int)label];
        }

        return new InferenceResult(label == 1, probability);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _session.Dispose();
            _disposed = true;
        }
    }
}

[SuppressMessage("Design", "CA1515:Consider making public types internal", Justification = "InferenceResult is the primary DTO for ML inference across the AHS.SaaS monorepo.")]
public record InferenceResult(bool IsExcursion, float Probability);
