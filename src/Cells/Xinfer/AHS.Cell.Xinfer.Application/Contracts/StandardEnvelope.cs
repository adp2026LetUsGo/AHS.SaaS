using System;
using System.Text.Json.Serialization;

namespace AHS.Cell.Xinfer.Application.Contracts;

public sealed record StandardEnvelope<T>(
    [property: JsonPropertyName("metadata")] EnvelopeMetadata Metadata,
    [property: JsonPropertyName("data")] T? Data,
    [property: JsonPropertyName("status")] EnvelopeStatus Status
);

public sealed record EnvelopeMetadata(
    [property: JsonPropertyName("cell_id")] string CellId,
    [property: JsonPropertyName("contract_version")] string ContractVersion,
    [property: JsonPropertyName("timestamp")] DateTimeOffset Timestamp,
    [property: JsonPropertyName("request_id")] string RequestId
);

public sealed record EnvelopeStatus(
    [property: JsonPropertyName("code")] int Code,
    [property: JsonPropertyName("error_code")] string? ErrorCode,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("trace_id")] string TraceId
);
