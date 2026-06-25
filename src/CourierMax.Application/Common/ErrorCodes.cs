namespace CourierMax.Application.Common;

public static class ErrorCodes
{
    public const string ShipmentNotFound = nameof(ShipmentNotFound);
    public const string VehicleNotFound = nameof(VehicleNotFound);
    public const string CityNotFound = nameof(CityNotFound);
    public const string DriverNotFound = nameof(DriverNotFound);
    public const string RouteNotFound = nameof(RouteNotFound);

    public const string InvalidStatusTransition = nameof(InvalidStatusTransition);
    public const string InvalidStatusForAssignment = nameof(InvalidStatusForAssignment);
    public const string VehicleCapacityExceeded = nameof(VehicleCapacityExceeded);
    public const string VehicleNotAvailable = nameof(VehicleNotAvailable);
    public const string VehicleNoDriver = nameof(VehicleNoDriver);
    public const string DriverNotActive = nameof(DriverNotActive);
    public const string CancellationReasonRequired = nameof(CancellationReasonRequired);
    public const string NoCapacityAvailable = nameof(NoCapacityAvailable);

    public const string TrackingCodeConflict = nameof(TrackingCodeConflict);
    public const string InvalidDimensions = nameof(InvalidDimensions);
    public const string RouteNotConfigured = nameof(RouteNotConfigured);
}
