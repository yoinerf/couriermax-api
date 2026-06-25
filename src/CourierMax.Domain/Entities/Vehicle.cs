using CourierMax.Domain.Common;
using CourierMax.Domain.Exceptions;

namespace CourierMax.Domain.Entities;

public class Vehicle
{
    public int Id { get; private set; }
    public string Plate { get; private set; } = string.Empty;
    public int? DriverId { get; private set; }
    public decimal CapacityWeight { get; private set; }   // kg
    public decimal CapacityVolume { get; private set; }   // m³
    public bool IsAvailable { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Navigation
    public Driver? Driver { get; private set; }

    private readonly List<Shipment> _shipments = [];

    /// <summary>Obtiene los envíos activos para este vehículo.</summary>
    public IReadOnlyList<Shipment> GetShipments() => _shipments.AsReadOnly();

    private Vehicle() { }

    public Vehicle(string plate, decimal capacityWeight, decimal capacityVolume, int? driverId = null, DateTime? createdAt = null)
    {
        if (string.IsNullOrWhiteSpace(plate))
            throw new DomainException("Vehicle plate cannot be empty.");
        if (capacityWeight <= 0)
            throw new DomainException("Capacity weight must be positive.");
        if (capacityVolume <= 0)
            throw new DomainException("Capacity volume must be positive.");

        Plate = plate.Trim().ToUpper();
        CapacityWeight = capacityWeight;
        CapacityVolume = capacityVolume;
        DriverId = driverId;
        IsAvailable = true;
        CreatedAt = createdAt ?? SystemTime.Now();
    }

    /// <summary>Carga de peso actual de envíos asignados (no cancelados, no entregados).</summary>
    public decimal CurrentWeightLoad =>
        _shipments
            .Where(s => s.Status != Enums.ShipmentStatus.Cancelled &&
                        s.Status != Enums.ShipmentStatus.Delivered)
            .Sum(s => s.PackageWeight);

    /// <summary>Carga de volumen actual en m3.</summary>
    public decimal CurrentVolumeLoad =>
        _shipments
            .Where(s => s.Status != Enums.ShipmentStatus.Cancelled &&
                        s.Status != Enums.ShipmentStatus.Delivered)
            .Sum(s => s.PackageVolumeM3);

    public decimal AvailableWeight => CapacityWeight - CurrentWeightLoad;
    public decimal AvailableVolume => CapacityVolume - CurrentVolumeLoad;

    /// <summary>
    /// Valida que el envío dado quepa en este vehículo.
    /// Lanza VehicleCapacityExceededException si no es así.
    /// </summary>
    public void EnsureCanAccommodate(decimal weightKg, decimal volumeM3)
    {
        if (weightKg > AvailableWeight)
            throw new VehicleCapacityExceededException(Plate,
                $"Weight {weightKg} kg exceeds available capacity {AvailableWeight:F2} kg");

        if (volumeM3 > AvailableVolume)
            throw new VehicleCapacityExceededException(Plate,
                $"Volume {volumeM3:F4} m³ exceeds available capacity {AvailableVolume:F4} m³");
    }

    public void AssignDriver(int driverId) => DriverId = driverId;

    public void SetUnavailable() => IsAvailable = false;

    public void SetAvailable() => IsAvailable = true;
}
