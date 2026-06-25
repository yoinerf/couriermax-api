using CourierMax.Application.DTOs;

namespace CourierMax.Application.Services;

public interface IVehicleService
{
    Task<IReadOnlyList<VehicleResponseDto>> GetAllWithShipmentsAsync(CancellationToken ct = default);
    Task<VehicleResponseDto?> GetByIdWithShipmentsAsync(int id, CancellationToken ct = default);
}
