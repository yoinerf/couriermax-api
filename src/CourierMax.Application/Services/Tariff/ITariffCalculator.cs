using CourierMax.Domain.Enums;

namespace CourierMax.Application.Services.Tariff;

public interface ITariffCalculator
{
    decimal Calculate(ServiceType serviceType, PackageType packageType, decimal weightKg, decimal distanceTariff);
}
