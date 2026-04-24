using System;
using System.Threading.Tasks;
using AHS.Cell.Xinfer.Application.Contracts;
using AHS.Common;

namespace AHS.Cell.Xinfer.Application.Ports;

public interface IInferenceService
{
    /// <summary>
    /// Processes a prediction request based on transport and environmental data.
    /// </summary>
    Task<AHS.Common.Result<InferenceOutput_v1>> PredictAsync(InferenceInput_v1 input, CancellationToken ct = default);

    /// <summary>
    /// Validates the input data against the business rules defined in input_v1.
    /// </summary>
    AHS.Common.Result Validate(InferenceInput_v1 input);
}
