using AHS.Web.BentoUI.Models;
using System.Text;
using System.Globalization;

namespace AHS.Web.BentoUI.Services;

public sealed class ShipmentService
{
    public async Task<List<ShipmentRecord>> LoadFromStreamAsync(Stream stream)
    {
        var records = new List<ShipmentRecord>();
        using var reader = new StreamReader(stream, Encoding.UTF8);
        
        // Skip header
        var header = await reader.ReadLineAsync();
        
        while (await reader.ReadLineAsync() is { } line)
        {
            var parts = line.Split(',');
            if (parts.Length < 10) continue;

            try
            {
                records.Add(new ShipmentRecord
                {
                    RouteId = parts[0],
                    Carrier = parts[1],
                    DepartureTime = parts[2],
                    TransitTimeHrs = double.Parse(parts[3], CultureInfo.InvariantCulture),
                    ExternalTempAvg = double.Parse(parts[4], CultureInfo.InvariantCulture),
                    PackagingType = parts[5],
                    ProductType = parts[6],
                    ShipmentSize = int.Parse(parts[7]),
                    DelayFlag = parts[8] == "1",
                    TempExcursion = parts[9] == "1"
                });
            }
            catch
            {
                // Silently skip malformed rows for stability
            }
        }

        return records;
    }
}
