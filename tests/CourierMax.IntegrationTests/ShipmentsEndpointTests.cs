using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CourierMax.Application.DTOs;
using CourierMax.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace CourierMax.IntegrationTests;

public class ShipmentsEndpointTests : IClassFixture<CourierMaxWebApplicationFactory>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ShipmentsEndpointTests(CourierMaxWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private static CreateShipmentDto ValidCreateDto() => new(
        SenderName: "Juan Pérez",
        SenderPhone: "3001234567",
        SenderAddress: "Calle 1 # 2-3, Bogotá",
        RecipientName: "María López",
        RecipientPhone: "6011234567",
        RecipientAddress: "Carrera 5 # 6-7, Medellín",
        PackageWeight: 3m,
        PackageDimensions: "30x20x15",
        PackageType: PackageType.Package,
        ServiceType: ServiceType.Standard,
        OriginCityId: 1,
        DestinationCityId: 2
    );

     
    // POST /api/shipments
     

    [Fact]
    public async Task CreateShipment_ValidPayload_Returns201WithTrackingCode()
    {
        var response = await _client.PostAsJsonAsync("/api/shipments", ValidCreateDto());

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadAsStringAsync();
        var dto = JsonSerializer.Deserialize<ShipmentResponseDto>(body, _jsonOptions);

        dto.Should().NotBeNull();
        dto!.TrackingCode.Should().StartWith("CM-");
        dto.Status.Should().Be("Created");
        dto.TotalCost.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateShipment_InvalidPhone_Returns400()
    {
        var dto = ValidCreateDto() with { SenderPhone = "1234567890" };
        var response = await _client.PostAsJsonAsync("/api/shipments", dto);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateShipment_WeightTooHigh_Returns400()
    {
        var dto = ValidCreateDto() with { PackageWeight = 150m };
        var response = await _client.PostAsJsonAsync("/api/shipments", dto);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

     
    // GET /api/shipments/{id}
     

    [Fact]
    public async Task GetById_ExistingShipment_Returns200()
    {
        // Create first
        var createResponse = await _client.PostAsJsonAsync("/api/shipments", ValidCreateDto());
        var created = JsonSerializer.Deserialize<ShipmentResponseDto>(
            await createResponse.Content.ReadAsStringAsync(), _jsonOptions)!;

        var response = await _client.GetAsync($"/api/shipments/{created.Id}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetById_NonExistent_Returns404()
    {
        var response = await _client.GetAsync("/api/shipments/999999");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

     
    // GET /api/shipments/tracking/{code}
     

    [Fact]
    public async Task GetByTrackingCode_Exists_Returns200()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/shipments", ValidCreateDto());
        var created = JsonSerializer.Deserialize<ShipmentResponseDto>(
            await createResponse.Content.ReadAsStringAsync(), _jsonOptions)!;

        var response = await _client.GetAsync($"/api/shipments/tracking/{created.TrackingCode}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

     
    // PATCH /api/shipments/{id}/status
     

    [Fact]
    public async Task UpdateStatus_ValidTransition_Returns200()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/shipments", ValidCreateDto());
        var created = JsonSerializer.Deserialize<ShipmentResponseDto>(
            await createResponse.Content.ReadAsStringAsync(), _jsonOptions)!;

        var assignDto = new AssignShipmentDto(VehicleId: 1, AssignedBy: "operator");
        await _client.PostAsJsonAsync($"/api/shipments/{created.Id}/assign", assignDto);

        var statusDto = new UpdateShipmentStatusDto(ShipmentStatus.InTransit, "driver1", null);
        var response = await _client.PatchAsJsonAsync($"/api/shipments/{created.Id}/status", statusDto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = JsonSerializer.Deserialize<ShipmentResponseDto>(
            await response.Content.ReadAsStringAsync(), _jsonOptions);
        body!.Status.Should().Be("InTransit");
    }

    [Fact]
    public async Task CancelShipment_WithValidReason_Returns200()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/shipments", ValidCreateDto());
        var created = JsonSerializer.Deserialize<ShipmentResponseDto>(
            await createResponse.Content.ReadAsStringAsync(), _jsonOptions)!;

        var statusDto = new UpdateShipmentStatusDto(ShipmentStatus.Cancelled, "admin", "Client changed their mind");
        var response = await _client.PatchAsJsonAsync($"/api/shipments/{created.Id}/status", statusDto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CancelShipment_WithoutReason_Returns400()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/shipments", ValidCreateDto());
        var created = JsonSerializer.Deserialize<ShipmentResponseDto>(
            await createResponse.Content.ReadAsStringAsync(), _jsonOptions)!;

        var statusDto = new UpdateShipmentStatusDto(ShipmentStatus.Cancelled, "admin", null);
        var response = await _client.PatchAsJsonAsync($"/api/shipments/{created.Id}/status", statusDto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

     
    // GET /api/reports/driver-performance
     

    [Fact]
    public async Task GetDriverPerformance_Returns200WithList()
    {
        var response = await _client.GetAsync("/api/reports/driver-performance");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        var list = JsonSerializer.Deserialize<List<DriverPerformanceDto>>(body, _jsonOptions);
        list.Should().NotBeNull();
    }

     
    // GET /api/vehicles
     

    [Fact]
    public async Task GetVehicles_Returns200WithList()
    {
        var response = await _client.GetAsync("/api/vehicles");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        var list = JsonSerializer.Deserialize<List<VehicleResponseDto>>(body, _jsonOptions);
        list.Should().NotBeNull();
    }
}
