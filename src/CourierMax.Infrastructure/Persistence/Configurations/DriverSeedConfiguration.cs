using CourierMax.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CourierMax.Infrastructure.Persistence.Configurations;

/// <summary>
/// Datos semilla para los Conductores y Vehículos de referencia de pruebaTecnica.md.
/// </summary>
public class DriverSeedConfiguration : IEntityTypeConfiguration<Driver>
{
    public void Configure(EntityTypeBuilder<Driver> builder)
    {
        // Ya configurado en DriverConfiguration — solo agregar semilla aquí vía SQL directo
        // La siembra (seed) se maneja a través de DbInitializer para evitar problemas de propiedades de sombra de EF con setters privados.
    }
}
