using Microsoft.AspNetCore.Mvc;
using AHS.Common.Models;
namespace AHS.Gateway.Api.Controllers;
[ApiController]
[Route("api/pharma/traceability")]
public class PharmaTraceabilityController : ControllerBase {
    [HttpGet("predict-risk")]
    public ActionResult<PredictionResponse> GetRisk() => Ok(new PredictionResponse(
        Guid.NewGuid().ToString(), 0.84f, "RISK", 14, DateTime.UtcNow, 0.95f, 0.92f, 0.935f,
        "Anomalous Temperature Deviation (Route Segment: T1)",
        new Dictionary<string, float> { { "Air Flow", 0.45f }, { "Ambient Temp", 0.35f }, { "Handling", 0.20f } }
    ));
}
