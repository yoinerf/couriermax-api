using CourierMax.Application.DTOs;
using CourierMax.Domain.Entities;

namespace CourierMax.Application.Mappings;

/// <summary>mapeo manual.</summary>
public static class ShipmentMappings
{
    public static ShipmentResponseDto ToResponseDto(this Shipment s) =>
        new(
            Id: s.Id,
            TrackingCode: s.TrackingCode,
            Status: s.Status.ToString(),
            ServiceType: s.ServiceType.ToString(),
            PackageType: s.PackageType.ToString(),
            PackageWeight: s.PackageWeight,
            PackageDimensions: s.PackageDimensions,
            TotalCost: s.TotalCost,
            Sender: new SenderRecipientDto(s.SenderName, s.SenderPhone, s.SenderAddress),
            Recipient: new SenderRecipientDto(s.RecipientName, s.RecipientPhone, s.RecipientAddress),
            OriginCity: s.OriginCity is not null
                ? new CityDto(s.OriginCity.Id, s.OriginCity.Name)
                : new CityDto(s.OriginCityId, string.Empty),
            DestinationCity: s.DestinationCity is not null
                ? new CityDto(s.DestinationCity.Id, s.DestinationCity.Name)
                : new CityDto(s.DestinationCityId, string.Empty),
            Vehicle: s.Vehicle is not null
                ? new VehicleAssignmentDto(s.Vehicle.Id, s.Vehicle.Plate, s.Vehicle.Driver?.Name)
                : null,
            CreatedAt: s.CreatedAt,
            AssignedAt: s.AssignedAt,
            DeliveredAt: s.DeliveredAt,
            History: s.History.Select(h => h.ToDto()).ToList()
        );

    public static ShipmentHistoryDto ToDto(this ShipmentHistory h) =>
        new(
            PreviousStatus: h.PreviousStatus?.ToString(),
            NewStatus: h.NewStatus.ToString(),
            Reason: h.Reason,
            ChangedBy: h.ChangedBy,
            CreatedAt: h.CreatedAt
        );

    public static VehicleResponseDto ToResponseDto(this Vehicle v) =>
        new(
            Id: v.Id,
            Plate: v.Plate,
            IsAvailable: v.IsAvailable,
            CapacityWeight: v.CapacityWeight,
            CapacityVolume: v.CapacityVolume,
            CurrentWeightLoad: v.CurrentWeightLoad,
            CurrentVolumeLoad: v.CurrentVolumeLoad,
            Driver: v.Driver is not null ? new DriverDto(v.Driver.Id, v.Driver.Name, v.Driver.IsActive) : null
        );
}
