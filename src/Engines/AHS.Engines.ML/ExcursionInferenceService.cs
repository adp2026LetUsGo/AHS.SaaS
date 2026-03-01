using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace AHS.Engines.ML;

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

    public PredictionResult Predict(float transitTime, float avgTemp, float delayFlag)
    {
        var inputTensor = new DenseTensor<float>(new[] { 1, 3 });
        inputTensor[0, 0] = transitTime;
        inputTensor[0, 1] = avgTemp;
        inputTensor[0, 2] = delayFlag;

        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("float_input", inputTensor)
        };

        using var results = _session.Run(inputs);
        
        var labelResult = results.FirstOrDefault(r => r.Name == "output_label");
        if (labelResult == null)
        {
            throw new InvalidOperationException("❌ GxP CRITICAL ERROR: Model output 'output_label' not found.");
        }

        var label = labelResult.AsEnumerable<long>().First();
        
        // Return 1 for High Risk, 0 for Normal based on training script logic
        return new PredictionResult(label == 1);
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

public record PredictionResult(bool IsHighRisk);
