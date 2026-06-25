using CourierMax.Domain.Interfaces;
using CourierMax.Domain.Interfaces.Repositories;
using CourierMax.Infrastructure.Persistence;
using CourierMax.Infrastructure.Repositories;
using CourierMax.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CourierMax.Infrastructure;

/// <summary>
/// Extensión de registro de Inyección de Dependencias (DI) para la capa de Infraestructura.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<CourierMaxDbContext>((sp, options) =>
        {
            if (configuration["UseSqlite"] == "true")
            {
                var conn = sp.GetService<System.Data.Common.DbConnection>();
                if (conn != null)
                {
                    options.UseSqlite(conn);
                }
                else
                {
                    options.UseSqlite(configuration.GetConnectionString("DefaultConnection"));
                }
            }
            else
            {
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    sqlOptions => sqlOptions.MigrationsAssembly(typeof(CourierMaxDbContext).Assembly.FullName));
            }
        });

        // Repositorios
        services.AddScoped<IShipmentRepository, ShipmentRepository>();
        services.AddScoped<IVehicleRepository, VehicleRepository>();
        services.AddScoped<IDriverRepository, DriverRepository>();
        services.AddScoped<ICityRepository, CityRepository>();
        services.AddScoped<IDistanceRepository, DistanceRepository>();

        // Unidad de Trabajo (Unit of Work)
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Servicios
        services.AddSingleton<IBusinessDayCalculator, BusinessDayCalculator>();

        return services;
    }
}
