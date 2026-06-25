using CourierMax.Domain.Entities;
using CourierMax.Domain.Enums;

namespace CourierMax.Application.Services.Tariff;

/// <summary>Interfaz para el cálculo de tarifas - Patrón .</summary>
public interface ITariffStrategy
{
    ServiceType ServiceType { get; }
    decimal GetBaseRate(ITariffConfiguration configuration);
}

public class StandardTariffStrategy : ITariffStrategy
{
    public ServiceType ServiceType => ServiceType.Standard;
    public decimal GetBaseRate(ITariffConfiguration configuration) => configuration.GetBaseRate(ServiceType);
}

public class ExpressTariffStrategy : ITariffStrategy
{
    public ServiceType ServiceType => ServiceType.Express;
    public decimal GetBaseRate(ITariffConfiguration configuration) => configuration.GetBaseRate(ServiceType);
}

public class SameDayTariffStrategy : ITariffStrategy
{
    public ServiceType ServiceType => ServiceType.SameDay;
    public decimal GetBaseRate(ITariffConfiguration configuration) => configuration.GetBaseRate(ServiceType);
}

/// <summary>
/// Calcula el costo total de un envío aplicando:
///  - Tarifa base por tipo de servicio
///  - Recargo por peso (> 2 kg: COP 1,500/kg)
///  - Tarifa por distancia según tabla de pares de ciudades
///  - Recargo por tipo de paquete (Frágil +30%, Perecedero +25%)
/// </summary>
public class TariffCalculatorService : ITariffCalculator
{
    private readonly Dictionary<ServiceType, ITariffStrategy> _strategies;
    private readonly ITariffConfiguration _configuration;

    public TariffCalculatorService(
        IEnumerable<ITariffStrategy> strategies,
        ITariffConfiguration configuration)
    {
        _strategies = strategies.ToDictionary(s => s.ServiceType);
        _configuration = configuration;
    }

    /// <summary>
    /// Calcula el costo total del envío.
    /// </summary>
    /// <param name="serviceType">Tipo de servicio.</param>
    /// <param name="packageType">Tipo de paquete para recargos.</param>
    /// <param name="weightKg">Peso del paquete en kg.</param>
    /// <param name="distanceTariff">Tarifa de distancia fija según ciudades.</param>
    public decimal Calculate(
        ServiceType serviceType,
        PackageType packageType,
        decimal weightKg,
        decimal distanceTariff)
    {
        if (!_strategies.TryGetValue(serviceType, out var strategy))
            throw new InvalidOperationException($"No tariff strategy registered for service type '{serviceType}'.");

        var baseCost = strategy.GetBaseRate(_configuration);
        var weightSurcharge = weightKg > _configuration.FreeWeightThresholdKg
            ? (weightKg - _configuration.FreeWeightThresholdKg) * _configuration.WeightSurchargePerKg
            : 0m;

        var subtotal = baseCost + weightSurcharge + distanceTariff;

        var packageSurcharge = packageType switch
        {
            PackageType.Fragile => subtotal * _configuration.FragileSurchargePercent,
            PackageType.Perishable => subtotal * _configuration.PerishableSurchargePercent,
            _ => 0m
        };

        return subtotal + packageSurcharge;
    }
}
