using System.Text.Json.Serialization;
using AHS.Common.Models;
namespace AHS.Common.Serialization;
[JsonSourceGenerationOptions(WriteIndented = false, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(PredictionResponse))]
[JsonSerializable(typeof(PredictRiskRequest))]
[JsonSerializable(typeof(Dictionary<string, float>))]
[JsonSerializable(typeof(List<PredictionResponse>))]
public partial class AotJsonContext : JsonSerializerContext { }
