using CourierMax.Domain.Enums;

namespace CourierMax.Application.DTOs;

// DTOs de Solicitud
public record CreateShipmentDto(
    string SenderName,
    string SenderPhone,
    string SenderAddress,
    string RecipientName,
    string RecipientPhone,
    string RecipientAddress,
    decimal PackageWeight,
    string PackageDimensions,   // "LxWxH" en cm
    PackageType PackageType,
    ServiceType ServiceType,
    int OriginCityId,
    int DestinationCityId
);

public record UpdateShipmentStatusDto(
    ShipmentStatus NewStatus,
    string ChangedBy,
    string? Reason
);

public record AssignShipmentDto(
    int? VehicleId,
    string AssignedBy
);

// DTOs de Respuesta
public record ShipmentResponseDto(
    int Id,
    string TrackingCode,
    string Status,
    string ServiceType,
    string PackageType,
    decimal PackageWeight,
    string PackageDimensions,
    decimal? TotalCost,
    SenderRecipientDto Sender,
    SenderRecipientDto Recipient,
    CityDto OriginCity,
    CityDto DestinationCity,
    VehicleAssignmentDto? Vehicle,
    DateTime CreatedAt,
    DateTime? AssignedAt,
    DateTime? DeliveredAt,
    IReadOnlyList<ShipmentHistoryDto> History
);

public record SenderRecipientDto(
    string Name,
    string Phone,
    string Address
);

public record ShipmentHistoryDto(
    string? PreviousStatus,
    string NewStatus,
    string? Reason,
    string ChangedBy,
    DateTime CreatedAt
);

public record VehicleAssignmentDto(
    int VehicleId,
    string Plate,
    string? DriverName
);

public record CityDto(int Id, string Name);

// DTOs de Envíos Atrasados / Reportes
public record OverdueShipmentDto(
    int Id,
    string TrackingCode,
    string ServiceType,
    int SlaBusinessDays,
    int ElapsedBusinessDays,
    int OverdueByDays,
    DateTime CreatedAt,
    string OriginCity,
    string DestinationCity
);

public record DriverPerformanceDto(
    int DriverId,
    string DriverName,
    int TotalAssigned,
    int TotalDelivered,
    int TotalCancelled,
    int TotalInTransit,
    double AverageDeliveryDays,
    double SlaCompliancePercent,
    decimal TotalWeightTransported
);

public record VehicleResponseDto(
    int Id,
    string Plate,
    bool IsAvailable,
    decimal CapacityWeight,
    decimal CapacityVolume,
    decimal CurrentWeightLoad,
    decimal CurrentVolumeLoad,
    DriverDto? Driver
);

public record DriverDto(int Id, string Name, bool IsActive);
