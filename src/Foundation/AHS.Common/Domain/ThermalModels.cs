// src/Foundation/AHS.Common/Domain/ThermalModels.cs
namespace AHS.Common.Domain;

public record PredictiveShieldMetrics(float CurrentDeltaT, float Slope, float ProjectedDeltaT30, float TimeToFailureMin, bool IsCritical, int CriticalCount = 0);
