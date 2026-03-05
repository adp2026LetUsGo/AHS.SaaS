using System.Numerics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace AHS.Engines.ML.Optimization;

public static class DataPerformanceProfiler
{
    /// <summary>
    /// Normalizes temperature readings using SIMD Vectorization.
    /// Processes 8-16 readings per CPU cycle (AVX2/AVX-512).
    /// </summary>
    public static void NormalizeTemperatures(Span<float> data, float min, float max)
    {
        int i = 0;
        if (!Vector.IsHardwareAccelerated || data.Length < Vector<float>.Count)
        {
            // Fallback for non-SIMD hardware
            for (; i < data.Length; i++)
                data[i] = (data[i] - min) / (max - min);
            return;
        }

        Vector<float> vMin = new Vector<float>(min);
        Vector<float> vRange = new Vector<float>(max - min);
        int vectorSize = Vector<float>.Count;
        i = 0;

        // Process in chunks of Vector size
        for (; i <= data.Length - vectorSize; i += vectorSize)
        {
            Vector<float> vData = new Vector<float>(data.Slice(i));
            Vector<float> vNormalized = (vData - vMin) / vRange;
            vNormalized.CopyTo(data.Slice(i));
        }

        // Clean up remaining elements
        for (; i < data.Length; i++)
            data[i] = (data[i] - min) / (max - min);
    }
}
