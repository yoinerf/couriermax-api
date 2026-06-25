using CourierMax.Application.Common;
using CourierMax.Application.DTOs;

namespace CourierMax.Application.Services;

public interface IShipmentService
{
    Task<Result<ShipmentResponseDto>> CreateAsync(CreateShipmentDto dto, CancellationToken ct = default);
    Task<Result<ShipmentResponseDto>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Result<ShipmentResponseDto>> GetByTrackingCodeAsync(string trackingCode, CancellationToken ct = default);
    Task<Result<ShipmentResponseDto>> UpdateStatusAsync(int id, UpdateShipmentStatusDto dto, CancellationToken ct = default);
    Task<Result<ShipmentResponseDto>> AssignToVehicleAsync(int id, AssignShipmentDto dto, CancellationToken ct = default);
    Task<Result<IReadOnlyList<OverdueShipmentDto>>> GetOverdueAsync(DateTime? from, DateTime? to, CancellationToken ct = default);
}
