using CourierMax.Application.Common;
using Microsoft.AspNetCore.Http;

namespace CourierMax.API.Services;

public interface IErrorCodeMapper
{
    int MapToStatusCode(string? errorCode);
}

public class ErrorCodeMapper : IErrorCodeMapper
{
    private static readonly Dictionary<string, int> _mapping = new()
    {
        { ErrorCodes.ShipmentNotFound, StatusCodes.Status404NotFound },
        { ErrorCodes.VehicleNotFound, StatusCodes.Status404NotFound },
        { ErrorCodes.CityNotFound, StatusCodes.Status404NotFound },
        { ErrorCodes.DriverNotFound, StatusCodes.Status404NotFound },
        { ErrorCodes.RouteNotFound, StatusCodes.Status404NotFound },
        { ErrorCodes.InvalidStatusTransition, StatusCodes.Status422UnprocessableEntity },
        { ErrorCodes.InvalidStatusForAssignment, StatusCodes.Status422UnprocessableEntity },
        { ErrorCodes.VehicleCapacityExceeded, StatusCodes.Status422UnprocessableEntity },
        { ErrorCodes.VehicleNotAvailable, StatusCodes.Status422UnprocessableEntity },
        { ErrorCodes.VehicleNoDriver, StatusCodes.Status422UnprocessableEntity },
        { ErrorCodes.DriverNotActive, StatusCodes.Status422UnprocessableEntity },
        { ErrorCodes.CancellationReasonRequired, StatusCodes.Status422UnprocessableEntity },
        { ErrorCodes.NoCapacityAvailable, StatusCodes.Status422UnprocessableEntity },
        { ErrorCodes.InvalidDimensions, StatusCodes.Status400BadRequest },
        { ErrorCodes.RouteNotConfigured, StatusCodes.Status422UnprocessableEntity },
        { ErrorCodes.TrackingCodeConflict, StatusCodes.Status409Conflict },
    };

    public int MapToStatusCode(string? errorCode)
        => errorCode is not null && _mapping.TryGetValue(errorCode, out var statusCode)
            ? statusCode
            : StatusCodes.Status400BadRequest;
}
