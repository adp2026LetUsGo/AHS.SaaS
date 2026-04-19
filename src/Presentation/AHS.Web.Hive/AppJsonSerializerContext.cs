using System.Text.Json.Serialization;
using AHS.Common.Domain;
using AHS.Web.Hive.Models;
namespace AHS.Web.Hive; [JsonSerializable(typeof(PredictRiskRequest))][JsonSerializable(typeof(PredictRiskResponse))][JsonSerializable(typeof(ShipmentRecord))][JsonSerializable(typeof(List<ShipmentRecord>))] internal sealed partial class AppJsonSerializerContext : JsonSerializerContext { }
