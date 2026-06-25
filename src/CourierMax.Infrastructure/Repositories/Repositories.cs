using CourierMax.Domain.Entities;
using CourierMax.Domain.Interfaces.Repositories;
using CourierMax.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CourierMax.Infrastructure.Repositories;

public class VehicleRepository : IVehicleRepository
{
    private readonly CourierMaxDbContext _context;

    public VehicleRepository(CourierMaxDbContext context) => _context = context;

    public async Task<Vehicle?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _context.Vehicles
            .Include(v => v.Driver)
            .FirstOrDefaultAsync(v => v.Id == id, ct);

    public async Task<Vehicle?> GetByIdWithShipmentsAsync(int id, CancellationToken ct = default)
        => await _context.Vehicles
            .Include(v => v.Driver)
            .Include("_shipments")
            .FirstOrDefaultAsync(v => v.Id == id, ct);

    public async Task<IReadOnlyList<Vehicle>> GetAllWithShipmentsAsync(CancellationToken ct = default)
        => await _context.Vehicles
            .Include(v => v.Driver)
            .Include("_shipments")
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Vehicle>> GetAvailableAsync(CancellationToken ct = default)
        => await _context.Vehicles
            .Include(v => v.Driver)
            .Where(v => v.IsAvailable)
            .ToListAsync(ct);

    public async Task<Vehicle?> GetLeastLoadedAvailableAsync(CancellationToken ct = default)
    {
        var available = await GetAvailableAsync(ct);
        return available
            .OrderBy(v => v.CurrentWeightLoad / v.CapacityWeight)
            .FirstOrDefault();
    }

    public void Update(Vehicle vehicle)
        => _context.Vehicles.Update(vehicle);
}

public class DriverRepository : IDriverRepository
{
    private readonly CourierMaxDbContext _context;

    public DriverRepository(CourierMaxDbContext context) => _context = context;

    public async Task<Driver?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _context.Drivers.FindAsync([id], ct);

    public async Task<IReadOnlyList<Driver>> GetActiveDriversAsync(CancellationToken ct = default)
        => await _context.Drivers.Where(d => d.IsActive).ToListAsync(ct);

    public async Task<IReadOnlyList<Driver>> GetAllAsync(CancellationToken ct = default)
        => await _context.Drivers.ToListAsync(ct);
}

public class CityRepository : ICityRepository
{
    private readonly CourierMaxDbContext _context;

    public CityRepository(CourierMaxDbContext context) => _context = context;

    public async Task<City?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _context.Cities.FindAsync([id], ct);

    public async Task<City?> GetByNameAsync(string name, CancellationToken ct = default)
        => await _context.Cities.FirstOrDefaultAsync(c => c.Name == name, ct);

    public async Task<IReadOnlyList<City>> GetAllAsync(CancellationToken ct = default)
        => await _context.Cities.ToListAsync(ct);

    public async Task<bool> ExistsAsync(int id, CancellationToken ct = default)
        => await _context.Cities.AnyAsync(c => c.Id == id, ct);
}

public class DistanceRepository : IDistanceRepository
{
    private readonly CourierMaxDbContext _context;

    public DistanceRepository(CourierMaxDbContext context) => _context = context;

    public async Task<Distance?> GetByRouteAsync(int originCityId, int destinationCityId, CancellationToken ct = default)
        => await _context.Distances
            .Include(d => d.OriginCity)
            .Include(d => d.DestinationCity)
            .FirstOrDefaultAsync(
                d => d.OriginCityId == originCityId && d.DestinationCityId == destinationCityId,
                ct);

    public async Task<IReadOnlyList<Distance>> GetAllAsync(CancellationToken ct = default)
        => await _context.Distances
            .Include(d => d.OriginCity)
            .Include(d => d.DestinationCity)
            .ToListAsync(ct);
}
