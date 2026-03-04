using System.Collections.Generic;
using System.Text.Json.Serialization;
using AHS.Common.Models;

namespace AHS.Common.Serialization;

[JsonSourceGenerationOptions(WriteIndented = false, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(AHS.Common.Models.PredictionResponse))]
[JsonSerializable(typeof(Dictionary<string, float>))]
public partial class AotJsonContext : JsonSerializerContext { }
