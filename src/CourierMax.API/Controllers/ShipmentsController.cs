using CourierMax.API.Filters;
using CourierMax.API.Services;
using CourierMax.Application.DTOs;
using CourierMax.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace CourierMax.API.Controllers;

[ApiController]
[Route("api/shipments")]
[Produces("application/json")]
public class ShipmentsController : ControllerBase
{
    private readonly IShipmentService _service;
    private readonly IErrorCodeMapper _errorCodeMapper;
    private readonly ILogger<ShipmentsController> _logger;

    public ShipmentsController(
        IShipmentService service,
        IErrorCodeMapper errorCodeMapper,
        ILogger<ShipmentsController> logger)
    {
        _service = service;
        _errorCodeMapper = errorCodeMapper;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ShipmentResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ServiceFilter(typeof(ValidationFilter<CreateShipmentDto>))]
    public async Task<IActionResult> Create([FromBody] CreateShipmentDto dto, CancellationToken ct)
    {
        var result = await _service.CreateAsync(dto, ct);

        return result.Match<IActionResult>(
            onSuccess: shipment => CreatedAtAction(nameof(GetById), new { id = shipment.Id }, shipment),
            onFailure: (error, code) => Problem(error, statusCode: _errorCodeMapper.MapToStatusCode(code), title: code));
    }

    [HttpGet("{id:int}", Name = nameof(GetById))]
    [ProducesResponseType(typeof(ShipmentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return result.Match<IActionResult>(
            onSuccess: Ok,
            onFailure: (error, code) => Problem(error, statusCode: _errorCodeMapper.MapToStatusCode(code), title: code));
    }

    [HttpGet("tracking/{code}")]
    [ProducesResponseType(typeof(ShipmentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByTrackingCode(string code, CancellationToken ct)
    {
        var result = await _service.GetByTrackingCodeAsync(code, ct);
        return result.Match<IActionResult>(
            onSuccess: Ok,
            onFailure: (error, code2) => Problem(error, statusCode: _errorCodeMapper.MapToStatusCode(code2), title: code2));
    }

    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(typeof(ShipmentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ServiceFilter(typeof(ValidationFilter<UpdateShipmentStatusDto>))]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateShipmentStatusDto dto, CancellationToken ct)
    {
        var result = await _service.UpdateStatusAsync(id, dto, ct);
        return result.Match<IActionResult>(
            onSuccess: Ok,
            onFailure: (error, code) => Problem(error, statusCode: _errorCodeMapper.MapToStatusCode(code), title: code));
    }

    [HttpPost("{id:int}/assign")]
    [ProducesResponseType(typeof(ShipmentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ServiceFilter(typeof(ValidationFilter<AssignShipmentDto>))]
    public async Task<IActionResult> Assign(int id, [FromBody] AssignShipmentDto dto, CancellationToken ct)
    {
        var result = await _service.AssignToVehicleAsync(id, dto, ct);
        return result.Match<IActionResult>(
            onSuccess: Ok,
            onFailure: (error, code) => Problem(error, statusCode: _errorCodeMapper.MapToStatusCode(code), title: code));
    }

    [HttpGet("overdue")]
    [ProducesResponseType(typeof(IReadOnlyList<OverdueShipmentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOverdue(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken ct)
    {
        var result = await _service.GetOverdueAsync(from, to, ct);
        return result.Match<IActionResult>(
            onSuccess: Ok,
            onFailure: (error, code) => Problem(error, statusCode: 500, title: code));
    }
}
