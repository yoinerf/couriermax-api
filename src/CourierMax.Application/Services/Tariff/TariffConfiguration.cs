using CourierMax.Domain.Enums;
using Microsoft.Extensions.Configuration;

namespace CourierMax.Application.Services.Tariff;

public class TariffConfiguration : ITariffConfiguration
{
    private readonly IConfiguration _configuration;

    public TariffConfiguration(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public decimal GetBaseRate(ServiceType serviceType) => serviceType switch
    {
        ServiceType.Standard => _configuration.GetValue<decimal>("Tariff:BaseRates:Standard"),
        ServiceType.Express => _configuration.GetValue<decimal>("Tariff:BaseRates:Express"),
        ServiceType.SameDay => _configuration.GetValue<decimal>("Tariff:BaseRates:SameDay"),
        _ => throw new InvalidOperationException($"No base rate configured for service type '{serviceType}'.")
    };

    public decimal WeightSurchargePerKg => _configuration.GetValue<decimal>("Tariff:WeightSurchargePerKg");
    public decimal FreeWeightThresholdKg => _configuration.GetValue<decimal>("Tariff:FreeWeightThresholdKg");
    public decimal FragileSurchargePercent => _configuration.GetValue<decimal>("Tariff:FragileSurchargePercent");
    public decimal PerishableSurchargePercent => _configuration.GetValue<decimal>("Tariff:PerishableSurchargePercent");
}
