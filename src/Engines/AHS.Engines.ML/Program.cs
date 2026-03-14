using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace AHS.Engines.ML;

internal sealed class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("🚀 AHS Native AOT Inference Engine Starting...");
        string modelPath = "excursion_risk_v1.onnx";
        if (!File.Exists(modelPath))
        {
            Console.WriteLine($"❌ Error: Model file not found at {modelPath}");
            return;
        }
        try
        {
            using var session = new InferenceSession(modelPath);
            // Default test input if no args provided
            float transitTime = args.Length > 0 ? float.Parse(args[0]) : 72.0f;
            float avgTemp = args.Length > 1 ? float.Parse(args[1]) : 28.5f;
            float delayFlag = args.Length > 2 ? float.Parse(args[2]) : 1.0f;
            Console.WriteLine($"📊 Input: Transit={transitTime}h, Temp={avgTemp}C, Delay={delayFlag}");
            var inputTensor = new DenseTensor<float>(new[] { 1, 3 });
            inputTensor[0, 0] = transitTime;
            inputTensor[0, 1] = avgTemp;
            inputTensor[0, 2] = delayFlag;
            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("float_input", inputTensor)
            };
            using var results = session.Run(inputs);
            Console.WriteLine("🧪 Output Names: " + string.Join(", ", results.Select(r => r.Name)));
            
            // skl2onnx standard outputs: 'output_label' (Int64) and 'output_probability' (Sequence of Maps)
            var labelResult = results.FirstOrDefault(r => r.Name == "output_label");
            var probResult = results.FirstOrDefault(r => r.Name == "output_probability");
            if (labelResult != null)
            {
                var label = labelResult.AsEnumerable<long>().First();
                Console.WriteLine($"🎯 Prediction: {(label == 1 ? "HIGH RISK" : "NORMAL")}");
            }
            Console.WriteLine("✅ Inference completed successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Inference failed: {ex.Message}");
            throw; // Rethrow to preserve stack trace and satisfy CA1031 in top-level Main
        }
    }
}
