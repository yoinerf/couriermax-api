using CourierMax.Domain.Entities;

namespace CourierMax.Domain.Interfaces.Repositories;

public interface IVehicleRepository
{
    Task<Vehicle?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Vehicle?> GetByIdWithShipmentsAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<Vehicle>> GetAllWithShipmentsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Vehicle>> GetAvailableAsync(CancellationToken ct = default);

    /// <summary>Retorna el vehículo disponible con la menor carga de peso actual (balanceo de carga).</summary>
    Task<Vehicle?> GetLeastLoadedAvailableAsync(CancellationToken ct = default);
    void Update(Vehicle vehicle);
}
