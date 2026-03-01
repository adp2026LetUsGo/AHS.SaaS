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

        using var results = _session.Run(inputs);
        
        // skl2onnx standard outputs: 'output_label' (Int64) and 'output_probability' (Sequence of Maps)
        var labelResult = results.FirstOrDefault(r => r.Name == "output_label");
        var probResult = results.FirstOrDefault(r => r.Name == "output_probability");

        if (labelResult == null)
        {
            throw new InvalidOperationException("❌ GxP CRITICAL ERROR: Model output 'output_label' not found.");
        }

        var label = labelResult.AsEnumerable<long>().First();
        float probability = 0.5f;

        // Extract probability if available (depends on scikit-learn model type and options)
        if (probResult != null)
        {
            // For RandomForestClassifier, it's often a sequence of maps (if zipmap=True) 
            // or an array of floats (if zipmap=False as I set in training script).
            // Since I set zipmap=False, it's a Tensor<float> of shape [batch, classes].
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

public record InferenceResult(bool IsExcursion, float Probability);
