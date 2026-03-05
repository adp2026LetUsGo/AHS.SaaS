using System.Net.Http.Json;
using AHS.Common.Models;
using AHS.Common.Serialization;

namespace AHS.Web.BentoUI.Services;

internal sealed class GatewayClient(HttpClient http)
{
    public async Task<PredictionResponse?> GetPharmaRiskAsync()
    {
        try 
        {
            // Explicit route matching the Controller [HttpGet("predict-risk")]
            return await http.GetFromJsonAsync<PredictionResponse>(
                "api/pharma/traceability/predict-risk", 
                AotJsonContext.Default.PredictionResponse).ConfigureAwait(false);
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Network Error: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"API Logic Error: {ex.Message}");
            return null;
        }
    }
}
