using CourierMax.Domain.Entities;
using CourierMax.Domain.Enums;
using CourierMax.Domain.Interfaces.Repositories;
using CourierMax.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CourierMax.Infrastructure.Repositories;

public class ShipmentRepository : IShipmentRepository
{
    private readonly CourierMaxDbContext _context;

    public ShipmentRepository(CourierMaxDbContext context) => _context = context;

    private IQueryable<Shipment> BuildShipmentQuery(bool tracking = true)
        => tracking
            ? _context.Shipments
            : _context.Shipments.AsNoTracking();

    private IQueryable<Shipment> BuildDetailedShipmentQuery(bool tracking = true)
        => BuildShipmentQuery(tracking)
            .Include(s => s.OriginCity)
            .Include(s => s.DestinationCity)
            .Include(s => s.Vehicle).ThenInclude(v => v!.Driver)
            .Include(s => s.History);

    public async Task<Shipment?> GetByIdAsync(int id, CancellationToken ct = default)
        => await BuildShipmentQuery().FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<Shipment?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default)
        => await BuildDetailedShipmentQuery().FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<Shipment?> GetByTrackingCodeAsync(string trackingCode, CancellationToken ct = default)
        => await BuildDetailedShipmentQuery().FirstOrDefaultAsync(s => s.TrackingCode == trackingCode, ct);

    public async Task<bool> ExistsByTrackingCodeAsync(string trackingCode, CancellationToken ct = default)
        => await _context.Shipments.AnyAsync(s => s.TrackingCode == trackingCode, ct);

    public async Task<IReadOnlyList<Shipment>> GetByStatusAsync(ShipmentStatus status, CancellationToken ct = default)
        => await _context.Shipments
            .Where(s => s.Status == status)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Shipment>> GetOverdueAsync(DateTime from, DateTime to, CancellationToken ct = default)
        => await BuildDetailedShipmentQuery(false)
            .Where(s =>
                s.CreatedAt >= from && s.CreatedAt <= to &&
                s.Status != ShipmentStatus.Delivered &&
                s.Status != ShipmentStatus.Cancelled)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Shipment>> GetByVehicleIdAsync(int vehicleId, CancellationToken ct = default)
        => await _context.Shipments
            .Where(s => s.VehicleId == vehicleId)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Shipment>> GetByDriverIdAsync(int driverId, CancellationToken ct = default)
        => await BuildShipmentQuery(false)
            .Include(s => s.Vehicle)
            .Where(s => s.Vehicle != null && s.Vehicle.DriverId == driverId)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Shipment>> GetByDriverIdsAsync(IEnumerable<int> driverIds, CancellationToken ct = default)
    {
        var ids = driverIds?.ToList() ?? [];

        if (ids.Count == 0)
            return [];

        return await BuildShipmentQuery(false)
            .Include(s => s.Vehicle)
            .Where(s => s.Vehicle != null && ids.Contains(s.Vehicle.DriverId!.Value))
            .ToListAsync(ct);
    }

    public async Task<int> GetNextSequenceAsync(CancellationToken ct = default)
    {
        var max = await _context.Shipments.AnyAsync(ct)
            ? await _context.Shipments.MaxAsync(s => s.Id, ct)
            : 0;
        return max + 1;
    }

    public async Task AddAsync(Shipment shipment, CancellationToken ct = default)
        => await _context.Shipments.AddAsync(shipment, ct);

    public void Update(Shipment shipment)
        => _context.Shipments.Update(shipment);
}
