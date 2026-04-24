// src/Cells/Xinfer/AHS.Cell.Xinfer.API/Endpoints/PredictEndpoints.cs
using AHS.Cell.Xinfer.Application.Contracts;
using AHS.Cell.Xinfer.Application.Ports;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using AHS.Cell.Xinfer.Application;

namespace AHS.Cell.Xinfer.API.Endpoints;

internal static class PredictEndpoints
{
    public static IEndpointRouteBuilder MapPredictEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/v1/predict", async (
            InferenceInput_v1 input, 
            IInferenceService service, 
            IConfiguration config, 
            HttpContext context,
            CancellationToken ct) =>
        {
            // 1. Cell Identity from Config
            var cellId = config["CellConfig:CellId"] ?? "xinfer";

            // 2. Traceability & Request ID
            string requestId = Guid.NewGuid().ToString();
            string traceId = context.Request.Headers["X-Trace-Id"].FirstOrDefault() 
                            ?? context.Request.Headers["traceparent"].FirstOrDefault() 
                            ?? requestId;

            // 3. Application-Layer Validation
            var validation = service.Validate(input);
            if (validation.IsFailure)
            {
                var errorEnvelope = new StandardEnvelope<object>(
                    new EnvelopeMetadata(cellId, "standard_envelope_v1", DateTimeOffset.UtcNow, requestId),
                    null,
                    new EnvelopeStatus(400, "ERR_VALIDATION_FAILED", validation.Error, traceId)
                );
                return Results.Json(errorEnvelope, XinferJsonContext.Default.StandardEnvelopeObject, statusCode: 400);
            }

            // 4. Processing
            var result = await service.PredictAsync(input, ct).ConfigureAwait(false);

            if (result.IsFailure)
            {
                int statusCode = result.Error.StartsWith("READINESS_FAIL") ? 422 : 400;
                var errorEnvelope = new StandardEnvelope<object>(
                    new EnvelopeMetadata(cellId, "standard_envelope_v1", DateTimeOffset.UtcNow, requestId),
                    null,
                    new EnvelopeStatus(statusCode, statusCode == 422 ? "ERR_READINESS_FAILED" : "ERR_PREDICTION_FAILED", result.Error, traceId)
                );
                return Results.Json(errorEnvelope, XinferJsonContext.Default.StandardEnvelopeObject, statusCode: statusCode);
            }

            // 5. Success Envelope
            var envelope = new StandardEnvelope<InferenceOutput_v1>(
                new EnvelopeMetadata(cellId, "inference_v1", DateTimeOffset.UtcNow, requestId),
                result.Value,
                new EnvelopeStatus(200, "OK", "Inference generated successfully", traceId)
            );

            return Results.Json(envelope, XinferJsonContext.Default.StandardEnvelopeInferenceOutput_v1);
        });

        return endpoints;
    }
}
