using Microsoft.AspNetCore.Mvc;
using AHS.Common.Models;

namespace AHS.Gateway.Api.Controllers;

[ApiController]
[Route("api/pharma/traceability")]
public class PharmaTraceabilityController : ControllerBase {
    [HttpGet("predict-risk")]
    public ActionResult<AHS.Common.Models.PredictionResponse> GetXaiRisk() => Ok(new AHS.Common.Models.PredictionResponse(
        Guid.NewGuid().ToString(),
        0.82f,
        "RISK DETECTED",
        14,
        DateTime.UtcNow,
        0.95f, 0.92f, 0.93f,
        "Anomalous Temperature Deviation",
        new Dictionary<string, float> { 
            { "Sensor SN-992", 0.85f }, 
            { "Ruta PTY-DAV", 0.62f }, 
            { "Carrier LogiTrans", 0.45f } 
        }
    ));
}
