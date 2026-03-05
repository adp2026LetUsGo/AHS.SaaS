using System.Text.Json.Serialization;
using AHS.Common.Models;
using System.Collections.Generic;

namespace AHS.Common.Serialization;

[JsonSourceGenerationOptions(WriteIndented = false, 
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(PredictionResponse))]
[JsonSerializable(typeof(Dictionary<string, float>))]
[JsonSerializable(typeof(List<PredictionResponse>))]
public partial class AotJsonContext : JsonSerializerContext
{
}
