using CourierMax.Domain.Enums;

namespace CourierMax.Application.Services.Tariff;

public interface ITariffConfiguration
{
    decimal GetBaseRate(ServiceType serviceType);
    decimal WeightSurchargePerKg { get; }
    decimal FreeWeightThresholdKg { get; }
    decimal FragileSurchargePercent { get; }
    decimal PerishableSurchargePercent { get; }
}
