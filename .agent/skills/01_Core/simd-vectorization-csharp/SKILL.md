---
name: simd-vectorization-csharp
description: >
  Expert guidance on SIMD (Single Instruction Multiple Data) vectorization in C# using
  System.Runtime.Intrinsics, Vector128<T>, Vector256<T>, Vector512<T>, and the cross-platform
  Vector<T> API. Use this skill whenever the user mentions SIMD, vectorization, AVX,
  AVX-512, SSE, intrinsics, Vector256, Vector512, hardware acceleration, parallel data
  processing, bulk memory operations, or performance-critical numeric/byte processing.
  Trigger on: SIMD, AVX, AVX-512, SSE, Vector128, Vector256, Vector512, intrinsics,
  vectorized loop, hardware accelerated, bulk byte processing.
---

# SIMD Vectorization in C# (.NET 10) — AHS.Engines.HPC

## Capa 5 — Zero-Allocation en Rutas Críticas

> `AHS.Engines.HPC` es el motor de física térmica. Capa 5 exige P99 < 10ms para el Oracle.
> Reglas para todo el código de este módulo:

```
✅ Span<T>           — procesa buffers de temperatura sin copias al heap
✅ stackalloc         — arrays temporales pequeños (≤256 elementos) en el stack
✅ readonly struct    — ThermalDataPoint en el stack, no en el heap
✅ ValueTask<T>       — en boundaries I/O, no en cálculos puros
❌ LINQ en hot path   — .Sum(), .Select(), .Where() = allocations + delegate invocations
❌ List<T> temporal   — usa Span<T> o ArrayPool<T>
❌ string interpolation en hot path — usa Span<char> con stackalloc
```

```csharp
// ✅ MKT con Span<T> — zero allocation para hasta 256 lecturas (stackalloc)
public static double CalculateMktZeroAlloc(ReadOnlySpan<double> celsius,
    double activationEnergy = 83_144.0)
{
    Span<double> exps = celsius.Length <= 256
        ? stackalloc double[celsius.Length]   // stack — zero GC pressure
        : new double[celsius.Length];          // heap fallback solo si es muy grande

    for (int i = 0; i < celsius.Length; i++)
        exps[i] = Math.Exp(-activationEnergy / (8.314 * (celsius[i] + 273.15)));

    double sumExp = 0;
    foreach (var e in exps) sumExp += e;      // ❌ NO: exps.Sum() — usa LINQ

    double mktK = -activationEnergy / (8.314 * Math.Log(sumExp / celsius.Length));
    return mktK - 273.15;
}

// ✅ ThermalDataPoint — readonly record struct, vive en el stack
public readonly record struct ThermalDataPoint(
    double CelsiusValue,
    DateTimeOffset Timestamp,
    string ZoneId);
// El Oracle consume ThermalDataPoint, nunca un objeto de sensor con detalles de protocolo
```

## API Hierarchy

```
System.Numerics.Vector<T>          ← Cross-platform, runtime width
System.Runtime.Intrinsics:
  Vector128<T>   (128-bit, SSE2/NEON)   — 16 bytes / 4 floats / 8 shorts
  Vector256<T>   (256-bit, AVX2)        — 32 bytes / 8 floats / 16 shorts
  Vector512<T>   (512-bit, AVX-512)     — 64 bytes / 16 floats / 32 shorts
```

---

## 1. Capability Detection

```csharp
using System.Runtime.Intrinsics.X86;
using System.Runtime.Intrinsics.Arm;

public static class SimdCapabilities
{
    public static readonly bool HasAvx512F   = Avx512F.IsSupported;
    public static readonly bool HasAvx512BW  = Avx512BW.IsSupported;   // byte/word ops
    public static readonly bool HasAvx512DQ  = Avx512DQ.IsSupported;   // dword/qword
    public static readonly bool HasAvx2      = Avx2.IsSupported;
    public static readonly bool HasSse42     = Sse42.IsSupported;
    public static readonly bool HasNeon      = AdvSimd.IsSupported;     // ARM

    // Prefer widest available
    public static int OptimalVectorWidth => HasAvx512F ? 512
                                          : HasAvx2    ? 256
                                                       : 128;
}
```

---

## 2. Vector512<T> — Core Operations

### Memory Loading

```csharp
// From pointer (unsafe)
float* ptr = ...;
Vector512<float> v = Vector512.Load(ptr);           // unaligned
Vector512<float> va = Vector512.LoadAligned(ptr);   // 64-byte aligned (faster)

// From span
ReadOnlySpan<float> span = ...;
Vector512<float> v = Vector512.Create(span);

// Broadcast scalar
Vector512<float> all42 = Vector512.Create(42.0f);   // [42,42,...,42] × 16
```

### Arithmetic

```csharp
Vector512<float> a = Vector512.Load(ptrA);
Vector512<float> b = Vector512.Load(ptrB);

Vector512<float> sum  = a + b;           // 16 adds in one instruction
Vector512<float> prod = a * b;
Vector512<float> div  = a / b;

// FMA (Fused Multiply-Add) — a*b + c, single rounding
Vector512<float> fma = Avx512F.FusedMultiplyAdd(a, b, c);
```

### Comparison & Masking (AVX-512 key feature)

```csharp
// AVX-512 uses mask registers — no blendv needed
Vector512<float> threshold = Vector512.Create(0.5f);
Vector512<float> data      = Vector512.Load(ptr);

// Create mask
Vector512<float> mask = Avx512F.Compare(data, threshold, FloatComparisonMode.OrderedGreaterThanNonSignaling);

// Blend / conditional select
Vector512<float> result = Avx512F.BlendVariable(Vector512<float>.Zero, data, mask);

// Or use masked operations directly (zeroing masking)
Vector512<float> zeroed = Avx512F.Add(data, b, mask); // only adds where mask == 1
```

---

## 3. Vectorized Loop Pattern

### Sum of float array

```csharp
public static float SumAvx512(ReadOnlySpan<float> data)
{
    float sum = 0f;

    if (Avx512F.IsSupported && data.Length >= Vector512<float>.Count)
    {
        var acc = Vector512<float>.Zero;
        int i = 0;
        int vectorized = data.Length - (data.Length % Vector512<float>.Count); // 16

        fixed (float* p = data)
        {
            for (; i < vectorized; i += Vector512<float>.Count)
                acc = Avx512F.Add(acc, Vector512.Load(p + i));
        }

        // Horizontal sum of acc (16 lanes → scalar)
        sum = Vector512.Sum(acc);
        // Scalar tail
        for (; i < data.Length; i++) sum += data[i];
    }
    else
    {
        foreach (var v in data) sum += v;
    }

    return sum;
}
```

---

## 4. Byte/String Processing — Vector512<byte>

### Find byte in large buffer

```csharp
public static int IndexOfByte(ReadOnlySpan<byte> buffer, byte target)
{
    if (!Avx512BW.IsSupported || buffer.Length < Vector512<byte>.Count)
        return buffer.IndexOf(target);

    var needle = Vector512.Create(target); // broadcast
    int i = 0;

    fixed (byte* p = buffer)
    {
        int vectorized = buffer.Length - (buffer.Length % Vector512<byte>.Count);

        for (; i < vectorized; i += Vector512<byte>.Count)
        {
            var chunk = Vector512.Load(p + i);
            var eq    = Avx512BW.CompareEqual(chunk, needle);

            // AVX-512: extract bitmask
            ulong mask = Avx512BW.MoveMask(eq); // 1 bit per byte lane
            if (mask != 0)
                return i + BitOperations.TrailingZeroCount(mask);
        }
    }

    // Scalar tail
    for (; i < buffer.Length; i++)
        if (buffer[i] == target) return i;

    return -1;
}
```

---

## 5. Cross-Platform with Vector<T>

```csharp
// Adapts to hardware width at runtime (128 or 256 bit)
public static void MultiplyAdd(Span<float> a, ReadOnlySpan<float> b, float scalar)
{
    int stride = Vector<float>.Count; // 4 or 8 depending on CPU
    var scalarVec = new Vector<float>(scalar);

    int i = 0;
    for (; i <= a.Length - stride; i += stride)
    {
        var va = new Vector<float>(a.Slice(i, stride));
        var vb = new Vector<float>(b.Slice(i, stride));
        (va + vb * scalarVec).CopyTo(a.Slice(i, stride));
    }

    // Scalar tail
    for (; i < a.Length; i++)
        a[i] += b[i] * scalar;
}
```

---

## 6. Memory Alignment — Getting Max Performance

```csharp
// Allocate 64-byte aligned memory for AVX-512
public static unsafe float[] AllocateAligned(int count)
{
    // .NET 8+ NativeMemory
    float* ptr = (float*)NativeMemory.AlignedAlloc(
        (nuint)(count * sizeof(float)),
        alignment: 64);  // 64 bytes for AVX-512

    // Wrap in array is not possible; use Span<float> instead
    return new Span<float>(ptr, count).ToArray(); // or just keep the pointer
}

// Better: use MemoryMarshal to work with aligned buffers
[StructLayout(LayoutKind.Sequential, Pack = 64)]
public struct AlignedBlock
{
    public fixed float Values[16]; // exactly one AVX-512 vector
}
```

---

## 7. Gather/Scatter — AVX-512 Specialty

```csharp
// Gather: load from non-contiguous indices
// gather[i] = source[indices[i]]
public static unsafe Vector512<float> GatherFloats(float* basePtr, Vector512<int> indices)
    => Avx512F.GatherVector512(basePtr, indices, scale: 4); // scale=sizeof(float)

// Scatter: store to non-contiguous indices  
public static unsafe void ScatterFloats(float* basePtr, Vector512<int> indices, Vector512<float> values)
    => Avx512F.Scatter(basePtr, indices, values, scale: 4);
```

---

## 8. Benchmarking Pattern (BenchmarkDotNet)

```csharp
[SimpleJob(RuntimeMoniker.Net90)]
[DisassemblyDiagnoser(maxDepth: 3)]   // View generated assembly
public class VectorBenchmarks
{
    private float[] _data = null!;

    [GlobalSetup]
    public void Setup() => _data = Enumerable.Range(0, 65536).Select(i => (float)i).ToArray();

    [Benchmark(Baseline = true)]
    public float Scalar()
    {
        float sum = 0;
        foreach (var v in _data) sum += v;
        return sum;
    }

    [Benchmark]
    public float Vectorized512() => SumAvx512(_data);
}
```

---

## 9. Common Patterns Cheat Sheet

| Task | API |
|---|---|
| Load 16 floats | `Vector512.Load(ptr)` |
| Store 16 floats | `Vector512.Store(ptr, vec)` |
| Add | `Avx512F.Add(a, b)` or `a + b` |
| Multiply | `Avx512F.Multiply(a, b)` or `a * b` |
| FMA | `Avx512F.FusedMultiplyAdd(a, b, c)` |
| Compare (mask) | `Avx512F.Compare(a, b, mode)` |
| Horizontal sum | `Vector512.Sum(vec)` |
| Shuffle bytes | `Avx512BW.PermuteVar32x16(vec, ctrl)` |
| Min/Max | `Avx512F.Min(a,b)` / `Avx512F.Max(a,b)` |
| Bitwise AND/OR | `Avx512F.And(a,b)` / `Avx512F.Or(a,b)` |

---

## Pitfalls

| Pitfall | Fix |
|---|---|
| Branch inside vectorized loop | Use masked ops instead |
| Unaligned load on strict hardware | Use `LoadAligned` where possible |
| Scalar tail missing | Always handle `Length % VectorWidth` remainder |
| JIT not vectorizing loop | Use explicit intrinsics or check with `[DisassemblyDiagnoser]` |
| Mixing float/double widths | Use explicit casts; `Vector512<double>` has only 8 lanes |
