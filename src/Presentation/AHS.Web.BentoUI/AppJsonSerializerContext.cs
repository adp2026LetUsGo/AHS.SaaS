using System.Text.Json.Serialization;
using AHS.Common;
using AHS.Web.BentoUI.Models;

namespace AHS.Web.BentoUI;

[JsonSerializable(typeof(PredictRiskRequest))]
[JsonSerializable(typeof(PredictRiskResponse))]
[JsonSerializable(typeof(ShipmentRecord))]
[JsonSerializable(typeof(List<ShipmentRecord>))]
internal sealed partial class AppJsonSerializerContext : JsonSerializerContext
{
}
