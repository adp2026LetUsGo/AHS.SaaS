using Microsoft.AspNetCore.Mvc;
using AHS.Common.Models;
namespace AHS.Gateway.Api.Controllers;
[ApiController]
[Route("api/pharma/traceability")]
public class PharmaTraceabilityController : ControllerBase {
    [HttpGet("predict-risk")]
    public ActionResult<AHS.Common.Models.PredictionResponse> GetRisk() => Ok(new AHS.Common.Models.PredictionResponse(Guid.NewGuid().ToString(), 0.99f, "SECURE", 8, DateTime.UtcNow));
}
