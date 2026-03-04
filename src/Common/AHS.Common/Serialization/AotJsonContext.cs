using System.Text.Json.Serialization;
using AHS.Common.Models;
namespace AHS.Common.Serialization;
[JsonSourceGenerationOptions(WriteIndented = false, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(AHS.Common.Models.PredictionResponse))]
public partial class AotJsonContext : JsonSerializerContext { }
