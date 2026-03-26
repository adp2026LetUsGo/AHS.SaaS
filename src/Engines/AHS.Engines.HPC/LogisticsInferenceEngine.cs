using AHS.Common.Domain;

namespace AHS.Engines.HPC;

public static class LogisticsInferenceEngine {
    public static LogisticsOracleResult PredictLogisticsRisk(ShipmentRiskProfile p, float currentTtf) {
        ArgumentNullException.ThrowIfNull(p);
        // Cálculo de Riesgo Ponderado (Lógica de Emiliano)
        // Ruta 40%, Carrier 30%, TempExt 30%
        float baseRisk = (p.RouteThreatLevel * 0.4f) + 
                         ((1 - p.CarrierReliability) * 0.3f) + 
                         (Math.Clamp(p.ExternalTempForecast / 40f, 0, 1) * 0.3f);
        
        // Factor Crítico: Si Insulation == Passive, añade +15% de riesgo base
        if (p.Insulation == InsulationType.Passive) baseRisk += 0.15f;
        
        float finalScore = Math.Clamp(baseRisk * 100, 0, 100);

        // Factor de Pesimismo: Reduce el TTF físico según el riesgo logístico
        // Pessimistic TTF: ActualTTF * (1.0 - (FinalScore / 200.0))
        float multiplier = 1.0f - (finalScore / 200f); 
        float pessimisticTtf = currentTtf * multiplier;

        string riskLevel = finalScore > 75 ? "CRITICAL" : 
                           finalScore > 40 ? "ELEVATED" : "NOMINAL";

        // XAI Logic: Identificar el mayor contribuyente
        float routeContrib = p.RouteThreatLevel * 0.4f;
        float carrierContrib = (1 - p.CarrierReliability) * 0.3f;
        float tempContrib = (Math.Clamp(p.ExternalTempForecast / 40f, 0, 1) * 0.3f);
        float insulationContrib = (p.Insulation == InsulationType.Passive ? 0.15f : 0);

        string insight = "NOMINAL_STABILITY";
        if (finalScore > 20) {
            if (insulationContrib > 0 && insulationContrib >= routeContrib && insulationContrib >= carrierContrib)
                insight = "INSULATION_TYPE";
            else if (routeContrib >= carrierContrib && routeContrib >= tempContrib)
                insight = "ROUTE_THREAT";
            else if (carrierContrib >= tempContrib)
                insight = "CARRIER_HISTORY";
            else
                insight = "THERMAL_EXPOSURE";
        }

        return new LogisticsOracleResult(finalScore, pessimisticTtf, riskLevel, insight);
    }
}
