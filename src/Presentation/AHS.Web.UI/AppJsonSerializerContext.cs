using System.Text.Json.Serialization;
using AHS.Common.Models;
using AHS.Web.UI.Models;
namespace AHS.Web.UI; [JsonSerializable(typeof(PredictRiskRequest))][JsonSerializable(typeof(PredictRiskResponse))][JsonSerializable(typeof(ShipmentRecord))][JsonSerializable(typeof(List<ShipmentRecord>))] internal sealed partial class AppJsonSerializerContext : JsonSerializerContext { }
