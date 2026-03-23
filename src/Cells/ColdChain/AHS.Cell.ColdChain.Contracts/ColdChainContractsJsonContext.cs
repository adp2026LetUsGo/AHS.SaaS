// src/Cells/ColdChain/AHS.Cell.ColdChain.Contracts/ColdChainContractsJsonContext.cs
using System.Text.Json.Serialization;

namespace AHS.Cell.ColdChain.Contracts;

[JsonSerializable(typeof(ShipmentExcursionDetected))]
[JsonSerializable(typeof(ShipmentSealed))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
public partial class ColdChainContractsJsonContext : JsonSerializerContext { }
