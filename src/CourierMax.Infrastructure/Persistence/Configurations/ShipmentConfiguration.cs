using CourierMax.Domain.Entities;
using CourierMax.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CourierMax.Infrastructure.Persistence.Configurations;

public class ShipmentConfiguration : IEntityTypeConfiguration<Shipment>
{
    public void Configure(EntityTypeBuilder<Shipment> builder)
    {
        builder.ToTable("Shipments");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.TrackingCode)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(s => s.TrackingCode).IsUnique();

        builder.Property(s => s.Status)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion(
                v => v.ToString(),
                v => Enum.Parse<ShipmentStatus>(v));

        builder.Property(s => s.ServiceType)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion(
                v => v.ToString(),
                v => Enum.Parse<ServiceType>(v));

        builder.Property(s => s.PackageType)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion(
                v => v.ToString(),
                v => Enum.Parse<PackageType>(v));

        builder.Property(s => s.PackageWeight)
            .HasColumnType("decimal(10,2)");

        builder.Property(s => s.PackageDimensions)
            .HasMaxLength(50);

        builder.Property(s => s.PackageVolumeM3)
            .HasColumnType("decimal(18,6)");

        builder.Property(s => s.SenderName).IsRequired().HasMaxLength(100);
        builder.Property(s => s.SenderPhone).IsRequired().HasMaxLength(15);
        builder.Property(s => s.SenderAddress).IsRequired().HasMaxLength(255);
        builder.Property(s => s.RecipientName).IsRequired().HasMaxLength(100);
        builder.Property(s => s.RecipientPhone).IsRequired().HasMaxLength(15);
        builder.Property(s => s.RecipientAddress).IsRequired().HasMaxLength(255);

        builder.Property(s => s.TotalCost).HasColumnType("decimal(18,2)");
        builder.Property(s => s.CreatedAt).IsRequired();

        // Relaciones
        builder.HasOne(s => s.OriginCity)
            .WithMany()
            .HasForeignKey(s => s.OriginCityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.DestinationCity)
            .WithMany()
            .HasForeignKey(s => s.DestinationCityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Vehicle)
            .WithMany("_shipments")
            .HasForeignKey(s => s.VehicleId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(s => s.History)
            .WithOne(h => h.Shipment)
            .HasForeignKey(h => h.ShipmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
