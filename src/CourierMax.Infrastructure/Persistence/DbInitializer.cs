using CourierMax.Domain.Entities;
using CourierMax.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CourierMax.Infrastructure.Persistence;

/// <summary>
/// Aplica migraciones pendientes y siembra (seed) datos de referencia al inicio.
/// </summary>
public static class DbInitializer
{
    public static async Task InitializeAsync(CourierMaxDbContext context, ILogger logger)
    {
        if (context.Database.IsSqlite())
        {
            await context.Database.EnsureCreatedAsync();
        }
        else
        {
            try
            {
                await context.Database.MigrateAsync();
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Migration failed (may already be up to date). Continuing...");
            }
        }

        await SeedCitiesAndDistancesAsync(context);
        await SeedDriversAndVehiclesAsync(context);
    }

    private static async Task SeedCitiesAndDistancesAsync(CourierMaxDbContext context)
    {
        if (await context.Cities.AnyAsync()) return;

        // 4 ciudades de referencia exactas de pruebaTecnica.md
        var bogota       = new City("Bogotá");
        var medellin     = new City("Medellín");
        var cali         = new City("Cali");
        var barranquilla = new City("Barranquilla");

        context.Cities.AddRange(bogota, medellin, cali, barranquilla);
        await context.SaveChangesAsync();

        // 6 rutas con distancias y tarifas exactas de pruebaTecnica.md
        var distances = new List<Distance>
        {
            new(bogota.Id,   medellin.Id,     480m, 12_000m),
            new(bogota.Id,   cali.Id,         360m,  9_000m),
            new(bogota.Id,   barranquilla.Id, 950m, 20_000m),
            new(medellin.Id, cali.Id,         310m,  8_000m),
            new(medellin.Id, barranquilla.Id, 650m, 15_000m),
            new(cali.Id,     barranquilla.Id, 900m, 18_000m),
        };

        context.Distances.AddRange(distances);
        await context.SaveChangesAsync();
    }

    private static async Task SeedDriversAndVehiclesAsync(CourierMaxDbContext context)
    {
        if (await context.Drivers.AnyAsync()) return;

        // Conductores de referencia de pruebaTecnica.md
        var juan  = new Driver("Juan Pérez");
        var maria = new Driver("María López");
        var carlo = new Driver("Carlos Ruiz");

        context.Drivers.AddRange(juan, maria, carlo);
        await context.SaveChangesAsync();

        // Vehículos con capacidad de la tabla de referencia
        var v1 = new Vehicle("ABC-123", 500m, 10m, juan.Id);
        var v2 = new Vehicle("DEF-456", 300m, 6m, maria.Id);
        var v3 = new Vehicle("GHI-789", 800m, 15m, carlo.Id);

        context.Vehicles.AddRange(v1, v2, v3);
        await context.SaveChangesAsync();
    }
}
