using CourierMax.Application.Common;
using CourierMax.Application.DTOs;

namespace CourierMax.Application.Services;

public interface IReportService
{
    Task<Result<IReadOnlyList<DriverPerformanceDto>>> GetAllDriverPerformanceAsync(CancellationToken ct = default);
    Task<Result<DriverPerformanceDto>> GetDriverPerformanceAsync(int driverId, CancellationToken ct = default);
}
