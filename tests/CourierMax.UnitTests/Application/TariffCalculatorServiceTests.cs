using CourierMax.Application.Services.Tariff;
using CourierMax.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using Xunit;

namespace CourierMax.UnitTests.Application;

public class TariffCalculatorServiceTests
{
    private readonly TariffCalculatorService _calculator;

    public TariffCalculatorServiceTests()
    {
        var strategies = new ITariffStrategy[]
        {
            new StandardTariffStrategy(),
            new ExpressTariffStrategy(),
            new SameDayTariffStrategy()
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Tariff:BaseRates:Standard"] = "8000",
                ["Tariff:BaseRates:Express"] = "15000",
                ["Tariff:BaseRates:SameDay"] = "25000",
                ["Tariff:WeightSurchargePerKg"] = "1500",
                ["Tariff:FreeWeightThresholdKg"] = "2",
                ["Tariff:FragileSurchargePercent"] = "0.30",
                ["Tariff:PerishableSurchargePercent"] = "0.25"
            })
            .Build();

        var tariffConfiguration = new TariffConfiguration(configuration);
        _calculator = new TariffCalculatorService(strategies, tariffConfiguration);
    }

    [Fact]
    public void Standard_NoExtraWeight_NoSurcharge_ReturnsBaseAndDistance()
    {
        // Base: 8.000 + 0 extra por peso + 9.000 distancia = 17.000
        var cost = _calculator.Calculate(ServiceType.Standard, PackageType.Package, 1m, 9_000m);
        cost.Should().Be(17_000m);
    }

    [Fact]
    public void Express_ExtraWeight_Fragile_CalculatesCorrectly()
    {
        // Ejemplo de pruebaTecnica.md:
        // Base expreso: 15.000
        // Extra por peso: 3 kg × 1.500 = 4.500
        // Distancia Bogotá-Medellín: 12.000
        // Subtotal: 31.500
        // Frágil +30%: 31.500 × 0.30 = 9.450
        // Total: 40.950
        var cost = _calculator.Calculate(ServiceType.Express, PackageType.Fragile, 5m, 12_000m);
        cost.Should().Be(40_950m);
    }

    [Fact]
    public void SameDay_Perishable_CalculatesCorrectly()
    {
        // Base: 25.000 + 0 extra (1 kg ≤ 2 kg) + 8.000 distancia = 33.000
        // Perecedero +25%: 33.000 × 0.25 = 8.250
        // Total: 41.250
        var cost = _calculator.Calculate(ServiceType.SameDay, PackageType.Perishable, 1m, 8_000m);
        cost.Should().Be(41_250m);
    }

    [Fact]
    public void NoExtraWeightSurcharge_WhenWeightAtExactLimit()
    {
        // Exactamente 2 kg — sin cargo adicional
        var costWith2kg = _calculator.Calculate(ServiceType.Standard, PackageType.Package, 2m, 0m);
        costWith2kg.Should().Be(8_000m);
    }

    [Fact]
    public void ExtraWeightSurcharge_AppliesForEachKgAbove2()
    {
        // 4 kg: 2 kg extra × 1.500 = 3.000
        var cost = _calculator.Calculate(ServiceType.Standard, PackageType.Package, 4m, 0m);
        cost.Should().Be(11_000m); // 8.000 + 3.000
    }

    [Fact]
    public void DocumentPackageType_HasNoSurcharge()
    {
        var doc = _calculator.Calculate(ServiceType.Standard, PackageType.Document, 1m, 0m);
        var pkg = _calculator.Calculate(ServiceType.Standard, PackageType.Package, 1m, 0m);
        doc.Should().Be(pkg);
    }

    [Theory]
    [InlineData(ServiceType.Standard, 8_000)]
    [InlineData(ServiceType.Express, 15_000)]
    [InlineData(ServiceType.SameDay, 25_000)]
    public void BaseRates_AreCorrectPerServiceType(ServiceType type, decimal expectedBase)
    {
        // 1 kg, sin distancia, sin recargo
        var cost = _calculator.Calculate(type, PackageType.Package, 1m, 0m);
        cost.Should().Be(expectedBase);
    }
}
