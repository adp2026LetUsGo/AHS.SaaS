// src/Cells/ColdChain/AHS.Cell.ColdChain.Infrastructure/Sensors/TemperatureMonitoringPipeline.cs
using System.Threading.Channels;
using AHS.Cell.ColdChain.Domain.Aggregates;

namespace AHS.Cell.ColdChain.Infrastructure.Sensors;

public sealed class TemperatureMonitoringPipeline
{
    private readonly Channel<TemperatureReading> _channel = Channel.CreateBounded<TemperatureReading>(
        new BoundedChannelOptions(10000) { FullMode = BoundedChannelFullMode.DropOldest });

    public ChannelWriter<TemperatureReading> Writer => _channel.Writer;

    public async Task StartProcessingAsync(CancellationToken ct)
    {
        await foreach (var reading in _channel.Reader.ReadAllAsync(ct))
        {
            // Logic to detect excursions and call RecordExcursionHandler
            if (reading.Value > 8.0 || reading.Value < 2.0)
            {
                // Trigger logic...
            }
        }
    }
}

public record TemperatureReading(Guid ShipmentId, string SensorId, double Value, DateTimeOffset Timestamp);
