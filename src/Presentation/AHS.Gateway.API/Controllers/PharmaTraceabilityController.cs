using Microsoft.AspNetCore.Mvc;
using AHS.Common.Models;

namespace AHS.Gateway.Api.Controllers;

[ApiController]
[Route("api/pharma/traceability")]
public class PharmaTraceabilityController : ControllerBase
{
    [HttpGet("predict-risk")]
    public ActionResult<PredictionResponse> GetRisk()
    {
        return Ok(new PredictionResponse(
            Guid.NewGuid().ToString(),
            0.84f,
            "STABLE",
            12,
            DateTime.UtcNow,
            0.95f, 
            0.92f, 
            0.93f,
            "CORS Verified GxP Feed",
            new Dictionary<string, float> { { "Connectivity", 1.0f } }
        ));
    }
}
