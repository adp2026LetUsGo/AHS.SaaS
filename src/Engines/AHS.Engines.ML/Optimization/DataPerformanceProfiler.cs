using System.Numerics;

namespace AHS.Engines.ML.Optimization;

internal static class DataPerformanceProfiler
{
    public static void NormalizeTemperatures(Span<float> data, float min, float max)
    {
        int i = 0;
        if (!Vector.IsHardwareAccelerated || data.Length < Vector<float>.Count)
        {
            for (; i < data.Length; i++)
                data[i] = (data[i] - min) / (max - min);
            return;
        }

        Vector<float> vMin = new Vector<float>(min);
        Vector<float> vRange = new Vector<float>(max - min);
        int vectorSize = Vector<float>.Count;
        i = 0;

        for (; i <= data.Length - vectorSize; i += vectorSize)
        {
            Vector<float> vData = new Vector<float>(data.Slice(i));
            Vector<float> vNormalized = (vData - vMin) / vRange;
            vNormalized.CopyTo(data.Slice(i));
        }

        for (; i < data.Length; i++)
            data[i] = (data[i] - min) / (max - min);
    }
}
