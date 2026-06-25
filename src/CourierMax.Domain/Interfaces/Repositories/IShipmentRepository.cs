using CourierMax.Domain.Entities;
using CourierMax.Domain.Enums;

namespace CourierMax.Domain.Interfaces.Repositories;

public interface IShipmentRepository
{
    Task<Shipment?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Shipment?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default);
    Task<Shipment?> GetByTrackingCodeAsync(string trackingCode, CancellationToken ct = default);
    Task<bool> ExistsByTrackingCodeAsync(string trackingCode, CancellationToken ct = default);
    Task<IReadOnlyList<Shipment>> GetByStatusAsync(ShipmentStatus status, CancellationToken ct = default);
    Task<IReadOnlyList<Shipment>> GetOverdueAsync(DateTime from, DateTime to, CancellationToken ct = default);
    Task<IReadOnlyList<Shipment>> GetByVehicleIdAsync(int vehicleId, CancellationToken ct = default);
    Task<IReadOnlyList<Shipment>> GetByDriverIdAsync(int driverId, CancellationToken ct = default);
    Task<IReadOnlyList<Shipment>> GetByDriverIdsAsync(IEnumerable<int> driverIds, CancellationToken ct = default);
    Task<int> GetNextSequenceAsync(CancellationToken ct = default);
    Task AddAsync(Shipment shipment, CancellationToken ct = default);
    void Update(Shipment shipment);
}
