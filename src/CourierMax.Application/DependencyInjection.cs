using FluentValidation;
using FluentValidation.Results;
using CourierMax.Application.Services;
using CourierMax.Application.Services.Tariff;
using CourierMax.Application.Validators;
using CourierMax.Domain.Common;
using Microsoft.Extensions.DependencyInjection;

namespace CourierMax.Application;

/// <summary>
/// Extensión de registro de Inyección de Dependencias (DI) para la capa de Aplicación.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Servicios de dominio
        services.AddScoped<IShipmentService, ShipmentService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IVehicleService, VehicleService>();
        services.AddScoped<IShipmentFactory, ShipmentFactory>();

        // Patrón par de Estrategia de Tarifas - Strategy 
        services.AddSingleton<ITariffConfiguration, TariffConfiguration>();
        services.AddSingleton<ITariffStrategy, StandardTariffStrategy>();
        services.AddSingleton<ITariffStrategy, ExpressTariffStrategy>();
        services.AddSingleton<ITariffStrategy, SameDayTariffStrategy>();
        services.AddSingleton<ITariffCalculator, TariffCalculatorService>();

        // Proveedor de tiempo del sistema inyectable
        services.AddSingleton<ISystemTime, SystemClock>();

        // Validadores
        services.AddScoped<IValidator<Application.DTOs.CreateShipmentDto>, CreateShipmentValidator>();
        services.AddScoped<IValidator<Application.DTOs.UpdateShipmentStatusDto>, UpdateShipmentStatusValidator>();
        services.AddScoped<IValidator<Application.DTOs.AssignShipmentDto>, AssignShipmentValidator>();

        return services;
    }
}
