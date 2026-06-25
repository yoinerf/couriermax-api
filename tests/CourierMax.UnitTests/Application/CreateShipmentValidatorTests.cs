using CourierMax.Application.DTOs;
using CourierMax.Application.Validators;
using CourierMax.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace CourierMax.UnitTests.Application;

public class CreateShipmentValidatorTests
{
    private readonly CreateShipmentValidator _validator = new();

    private static CreateShipmentDto Valid() => new(
        SenderName: "Juan Pérez",
        SenderPhone: "3001234567",
        SenderAddress: "Calle 1 # 2-3, Bogotá",
        RecipientName: "María López",
        RecipientPhone: "6011234567",
        RecipientAddress: "Carrera 5 # 6-7, Medellín",
        PackageWeight: 5m,
        PackageDimensions: "30x20x15",
        PackageType: PackageType.Package,
        ServiceType: ServiceType.Standard,
        OriginCityId: 1,
        DestinationCityId: 2
    );

    [Fact]
    public async Task ValidDto_ShouldPassValidation()
    {
        var result = await _validator.ValidateAsync(Valid());
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("1234567890")]   // Empieza con 1 — inválido
    [InlineData("2009876543")]   // Empieza con 2 — inválido
    [InlineData("300123456")]    // Solo 9 dígitos
    [InlineData("30012345678")]  // 11 dígitos
    [InlineData("")]
    [InlineData(null)]
    public async Task InvalidPhone_ShouldFailValidation(string? phone)
    {
        var dto = Valid() with { SenderPhone = phone! };
        var result = await _validator.ValidateAsync(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.SenderPhone));
    }

    [Theory]
    [InlineData("3001234567")]   // Empieza con 3 ✓
    [InlineData("3219876543")]   // Empieza con 3 ✓
    [InlineData("6011234567")]   // Empieza con 6 ✓
    public async Task ValidColombianPhone_ShouldPassValidation(string phone)
    {
        var dto = Valid() with { SenderPhone = phone };
        var result = await _validator.ValidateAsync(dto);
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(dto.SenderPhone));
    }

    [Theory]
    [InlineData(0.05)]   // Por debajo del mínimo
    [InlineData(100.1)]  // Por encima del máximo
    [InlineData(0)]
    public async Task InvalidWeight_ShouldFailValidation(double weight)
    {
        var dto = Valid() with { PackageWeight = (decimal)weight };
        var result = await _validator.ValidateAsync(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.PackageWeight));
    }

    [Theory]
    [InlineData(0.1)]
    [InlineData(1.0)]
    [InlineData(50.0)]
    [InlineData(100.0)]
    public async Task ValidWeight_ShouldPassValidation(double weight)
    {
        var dto = Valid() with { PackageWeight = (decimal)weight };
        var result = await _validator.ValidateAsync(dto);
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(dto.PackageWeight));
    }

    [Theory]
    [InlineData("0x20x15")]    // 0 cm — inválido
    [InlineData("201x20x15")] // 201 cm — supera el máximo
    [InlineData("abc")]       // Formato no válido
    [InlineData("30x20")]     // Falta la altura
    public async Task InvalidDimensions_ShouldFailValidation(string dims)
    {
        var dto = Valid() with { PackageDimensions = dims };
        var result = await _validator.ValidateAsync(dto);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task SameCityOriginAndDestination_ShouldFailValidation()
    {
        var dto = Valid() with { OriginCityId = 1, DestinationCityId = 1 };
        var result = await _validator.ValidateAsync(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.DestinationCityId));
    }

    [Fact]
    public async Task EmptySenderAddress_ShouldFailValidation()
    {
        var dto = Valid() with { SenderAddress = "" };
        var result = await _validator.ValidateAsync(dto);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task CancellationReasonTooShort_ShouldFailUpdateStatusValidation()
    {
        var validator = new UpdateShipmentStatusValidator();
        var dto = new UpdateShipmentStatusDto(ShipmentStatus.Cancelled, "admin", "No");
        var result = await validator.ValidateAsync(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.Reason));
    }

    [Fact]
    public async Task CancellationWithValidReason_ShouldPass()
    {
        var validator = new UpdateShipmentStatusValidator();
        var dto = new UpdateShipmentStatusDto(ShipmentStatus.Cancelled, "admin", "Client requested cancellation");
        var result = await validator.ValidateAsync(dto);
        result.IsValid.Should().BeTrue();
    }
}
