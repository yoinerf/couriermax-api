namespace CourierMax.Domain.Exceptions;

public class VehicleCapacityExceededException : DomainException
{
    public VehicleCapacityExceededException(string vehiclePlate, string reason)
        : base($"Vehicle '{vehiclePlate}' capacity exceeded: {reason}.", "VEHICLE_CAPACITY_EXCEEDED")
    {
    }
}
