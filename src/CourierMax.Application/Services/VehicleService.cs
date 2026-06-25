using CourierMax.Application.DTOs;
using CourierMax.Application.Mappings;
using CourierMax.Domain.Interfaces.Repositories;

namespace CourierMax.Application.Services;

public class VehicleService : IVehicleService
{
    private readonly IVehicleRepository _vehicles;

    public VehicleService(IVehicleRepository vehicles)
    {
        _vehicles = vehicles;
    }

    public async Task<IReadOnlyList<VehicleResponseDto>> GetAllWithShipmentsAsync(CancellationToken ct = default)
    {
        var vehicles = await _vehicles.GetAllWithShipmentsAsync(ct);
        return vehicles.Select(v => v.ToResponseDto()).ToList();
    }

    public async Task<VehicleResponseDto?> GetByIdWithShipmentsAsync(int id, CancellationToken ct = default)
    {
        var vehicle = await _vehicles.GetByIdWithShipmentsAsync(id, ct);
        return vehicle?.ToResponseDto();
    }
}
