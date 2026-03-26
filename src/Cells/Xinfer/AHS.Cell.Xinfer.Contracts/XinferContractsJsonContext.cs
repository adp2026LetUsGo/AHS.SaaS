// src/Cells/Xinfer/AHS.Cell.Xinfer.Contracts/XinferContractsJsonContext.cs
using System.Text.Json.Serialization;

namespace AHS.Cell.Xinfer.Contracts;

[JsonSerializable(typeof(ShipmentExcursionDetected))]
[JsonSerializable(typeof(ShipmentSealed))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
public partial class XinferContractsJsonContext : JsonSerializerContext { }
