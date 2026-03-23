// src/Foundation/AHS.Common/Engines/ThermalDataPoint.cs
namespace AHS.Common.Engines;

// readonly record struct — stack allocated, zero heap pressure in hot path
public readonly record struct ThermalDataPoint(
    double         CelsiusValue,
    DateTimeOffset Timestamp,
    string         ZoneId);
