using System.Net.Http.Json;
using AHS.Common.Models;
using AHS.Common.Serialization;

using System.Diagnostics.CodeAnalysis;

namespace AHS.Web.BentoUI.Services;

[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "GatewayClient is instantiated by the Blazor dependency injection container.")]
internal sealed class GatewayClient(HttpClient http)
{
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Top-level boundary for service calls to prevent UI crashes. Errors are logged to console.")]
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
