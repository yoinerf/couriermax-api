using CourierMax.Domain.Entities;
using CourierMax.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CourierMax.Infrastructure.Persistence.Configurations;

public class ShipmentHistoryConfiguration : IEntityTypeConfiguration<ShipmentHistory>
{
    public void Configure(EntityTypeBuilder<ShipmentHistory> builder)
    {
        builder.ToTable("ShipmentHistories");
        builder.HasKey(h => h.Id);

        builder.Property(h => h.PreviousStatus)
            .HasMaxLength(20)
            .HasConversion(
                v => v.HasValue ? v.ToString() : null,
                v => v != null ? Enum.Parse<ShipmentStatus>(v) : (ShipmentStatus?)null);

        builder.Property(h => h.NewStatus)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion(
                v => v.ToString(),
                v => Enum.Parse<ShipmentStatus>(v));

        builder.Property(h => h.Reason).HasMaxLength(500);
        builder.Property(h => h.ChangedBy).IsRequired().HasMaxLength(100);
        builder.Property(h => h.CreatedAt).IsRequired();
    }
}

public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("Vehicles");
        builder.HasKey(v => v.Id);

        builder.Property(v => v.Plate).IsRequired().HasMaxLength(20);
        builder.HasIndex(v => v.Plate).IsUnique();

        builder.Property(v => v.CapacityWeight).HasColumnType("decimal(10,2)").IsRequired();
        builder.Property(v => v.CapacityVolume).HasColumnType("decimal(10,2)").IsRequired();
        builder.Property(v => v.CreatedAt).IsRequired();

        builder.HasOne(v => v.Driver)
            .WithOne(d => d.Vehicle)
            .HasForeignKey<Vehicle>(v => v.DriverId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class DriverConfiguration : IEntityTypeConfiguration<Driver>
{
    public void Configure(EntityTypeBuilder<Driver> builder)
    {
        builder.ToTable("Drivers");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Name).IsRequired().HasMaxLength(100);
        builder.Property(d => d.CreatedAt).IsRequired();
    }
}

public class CityConfiguration : IEntityTypeConfiguration<City>
{
    public void Configure(EntityTypeBuilder<City> builder)
    {
        builder.ToTable("Cities");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(100);
        builder.HasIndex(c => c.Name).IsUnique();

        // Datos semilla — ciudades de referencia
        builder.HasData(
            new { Id = 1, Name = "Bogotá" },
            new { Id = 2, Name = "Medellín" },
            new { Id = 3, Name = "Cali" },
            new { Id = 4, Name = "Barranquilla" }
        );
    }
}

public class DistanceConfiguration : IEntityTypeConfiguration<Distance>
{
    public void Configure(EntityTypeBuilder<Distance> builder)
    {
        builder.ToTable("Distances");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.DistanceKm).HasColumnType("decimal(10,2)").IsRequired();
        builder.Property(d => d.Tariff).HasColumnType("decimal(18,2)").IsRequired();

        builder.HasOne(d => d.OriginCity)
            .WithMany()
            .HasForeignKey(d => d.OriginCityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.DestinationCity)
            .WithMany()
            .HasForeignKey(d => d.DestinationCityId)
            .OnDelete(DeleteBehavior.Restrict);

        // Datos semilla — rutas bidireccionales (tabla de referencia de pruebaTecnica.md)
        builder.HasData(
            // Bogotá (1) ↔ Medellín (2)
            new { Id = 1, OriginCityId = 1, DestinationCityId = 2, DistanceKm = 480m, Tariff = 12_000m },
            new { Id = 2, OriginCityId = 2, DestinationCityId = 1, DistanceKm = 480m, Tariff = 12_000m },
            // Bogotá (1) ↔ Cali (3)
            new { Id = 3, OriginCityId = 1, DestinationCityId = 3, DistanceKm = 360m, Tariff = 9_000m },
            new { Id = 4, OriginCityId = 3, DestinationCityId = 1, DistanceKm = 360m, Tariff = 9_000m },
            // Bogotá (1) ↔ Barranquilla (4)
            new { Id = 5, OriginCityId = 1, DestinationCityId = 4, DistanceKm = 950m, Tariff = 20_000m },
            new { Id = 6, OriginCityId = 4, DestinationCityId = 1, DistanceKm = 950m, Tariff = 20_000m },
            // Medellín (2) ↔ Cali (3)
            new { Id = 7, OriginCityId = 2, DestinationCityId = 3, DistanceKm = 310m, Tariff = 8_000m },
            new { Id = 8, OriginCityId = 3, DestinationCityId = 2, DistanceKm = 310m, Tariff = 8_000m },
            // Medellín (2) ↔ Barranquilla (4)
            new { Id = 9, OriginCityId = 2, DestinationCityId = 4, DistanceKm = 650m, Tariff = 15_000m },
            new { Id = 10, OriginCityId = 4, DestinationCityId = 2, DistanceKm = 650m, Tariff = 15_000m },
            // Cali (3) ↔ Barranquilla (4)
            new { Id = 11, OriginCityId = 3, DestinationCityId = 4, DistanceKm = 900m, Tariff = 18_000m },
            new { Id = 12, OriginCityId = 4, DestinationCityId = 3, DistanceKm = 900m, Tariff = 18_000m }
        );
    }
}
