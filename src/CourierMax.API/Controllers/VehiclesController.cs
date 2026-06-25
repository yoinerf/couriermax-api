using CourierMax.Application.DTOs;
using CourierMax.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace CourierMax.API.Controllers;

[ApiController]
[Route("api/vehicles")]
[Produces("application/json")]
public class VehiclesController : ControllerBase
{
    private readonly IVehicleService _vehicles;

    public VehiclesController(IVehicleService vehicles) => _vehicles = vehicles;

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<VehicleResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var vehicles = await _vehicles.GetAllWithShipmentsAsync(ct);
        return Ok(vehicles);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(VehicleResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var vehicle = await _vehicles.GetByIdWithShipmentsAsync(id, ct);
        if (vehicle is null)
            return Problem($"Vehicle with ID {id} not found.", statusCode: 404, title: "VEHICLE_NOT_FOUND");

        return Ok(vehicle);
    }
}
