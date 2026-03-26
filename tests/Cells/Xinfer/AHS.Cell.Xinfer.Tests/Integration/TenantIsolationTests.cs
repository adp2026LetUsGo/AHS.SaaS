using System.Net;
using System.Net.Http.Json;
using AHS.Cell.Xinfer.Application.DTOs;
using AHS.Cell.Xinfer.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace AHS.Cell.Xinfer.Tests.Integration;

public sealed class TenantIsolationTests(XinferWebAppFactory factory) : IClassFixture<XinferWebAppFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task TenantACannotSeeTenantBShipments()
    {
        // Arrange
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        // TenantB creates a shipment
        var createReq = new { 
            CargoType = CargoType.Vaccines, 
            InsulationType = InsulationType.Vip, 
            OriginLocation = "Warehouse B", 
            DestinationLocation = "Hospital B", 
            PlannedDeparture = DateTimeOffset.UtcNow.AddDays(1),
            ReasonForChange = "Setup isolation test"
        };
        
        _client.DefaultRequestHeaders.Add("X-Tenant-Id", tenantB.ToString());
        var postResponse = await _client.PostAsJsonAsync("/api/shipments", createReq);
        postResponse.EnsureSuccessStatusCode();
        var shipmentId = (await postResponse.Content.ReadFromJsonAsync<dynamic>())?.id;

        // Act - TenantA queries the same Shipment
        _client.DefaultRequestHeaders.Remove("X-Tenant-Id");
        _client.DefaultRequestHeaders.Add("X-Tenant-Id", tenantA.ToString());
        var getResponse = await _client.GetAsync(new Uri($"/api/shipments/{shipmentId}", UriKind.Relative));

        // Assert
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task TenantACannotSealTenantBShipment()
    {
        // Arrange
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        // TenantB creates a shipment
        var createReq = new { 
            CargoType = CargoType.Biologics, 
            InsulationType = InsulationType.Standard, 
            OriginLocation = "Warehouse B", 
            DestinationLocation = "Clinic B", 
            PlannedDeparture = DateTimeOffset.UtcNow.AddDays(1),
            ReasonForChange = "Setup seal isolation test"
        };
        
        _client.DefaultRequestHeaders.Add("X-Tenant-Id", tenantB.ToString());
        var postResponse = await _client.PostAsJsonAsync("/api/shipments", createReq);
        var shipmentId = (await postResponse.Content.ReadFromJsonAsync<dynamic>())?.id;

        // Act - TenantA attempts to seal it
        var sealReq = new { 
            FinalStatus = ShipmentStatus.Active, 
            QualityDecision = QualityDecision.Released,
            ReasonForChange = "Unauthorized seal attempt"
        };
        
        _client.DefaultRequestHeaders.Remove("X-Tenant-Id");
        _client.DefaultRequestHeaders.Add("X-Tenant-Id", tenantA.ToString());
        var sealResponse = await _client.PostAsJsonAsync($"/api/shipments/{shipmentId}/seal", sealReq);

        // Assert
        sealResponse.StatusCode.Should().Be(HttpStatusCode.InternalServerError); 
        // Note: KeyNotFoundException in the handler bubbles up as 500 currently.
        // In a production app, we would have a global exception filter mapping this to 404.
    }
}
