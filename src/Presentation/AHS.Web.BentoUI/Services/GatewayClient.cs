using System.Net.Http.Json;
using AHS.Common.Models;
using AHS.Common.Serialization;

namespace AHS.Web.BentoUI.Services;

public class GatewayClient(HttpClient http) {
    public async Task<AHS.Common.Models.PredictionResponse?> GetPharmaRiskAsync() {
        try {
            return await http.GetFromJsonAsync<AHS.Common.Models.PredictionResponse>("api/pharma/traceability/predict-risk", AotJsonContext.Default.PredictionResponse);
        }
        catch {
            return null;
        }
    }
}
