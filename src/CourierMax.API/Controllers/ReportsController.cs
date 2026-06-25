using CourierMax.API.Services;
using CourierMax.Application.DTOs;
using CourierMax.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace CourierMax.API.Controllers;

[ApiController]
[Route("api/reports")]
[Produces("application/json")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly IErrorCodeMapper _errorCodeMapper;

    public ReportsController(IReportService reportService, IErrorCodeMapper errorCodeMapper)
    {
        _reportService = reportService;
        _errorCodeMapper = errorCodeMapper;
    }

    [HttpGet("driver-performance")]
    [ProducesResponseType(typeof(IReadOnlyList<DriverPerformanceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _reportService.GetAllDriverPerformanceAsync(ct);
        return result.Match<IActionResult>(
            onSuccess: Ok,
            onFailure: (error, code) => Problem(error, statusCode: 500, title: code));
    }

    [HttpGet("driver-performance/{driverId:int}")]
    [ProducesResponseType(typeof(DriverPerformanceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByDriver(int driverId, CancellationToken ct)
    {
        var result = await _reportService.GetDriverPerformanceAsync(driverId, ct);
        return result.Match<IActionResult>(
            onSuccess: Ok,
            onFailure: (error, code) => Problem(error,
                statusCode: _errorCodeMapper.MapToStatusCode(code),
                title: code));
    }
}
