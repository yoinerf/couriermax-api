using CourierMax.Domain.Entities;
using CourierMax.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CourierMax.Infrastructure.Persistence;

public class CourierMaxDbContext : DbContext
{
    public CourierMaxDbContext(DbContextOptions<CourierMaxDbContext> options) : base(options) { }

    public DbSet<Shipment> Shipments => Set<Shipment>();
    public DbSet<ShipmentHistory> ShipmentHistories => Set<ShipmentHistory>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Driver> Drivers => Set<Driver>();
    public DbSet<City> Cities => Set<City>();
    public DbSet<Distance> Distances => Set<Distance>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CourierMaxDbContext).Assembly);
    }
}
