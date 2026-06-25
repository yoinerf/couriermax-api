using CourierMax.Domain.Exceptions;

namespace CourierMax.Domain.Entities;

public class Distance
{
    public int Id { get; private set; }
    public int OriginCityId { get; private set; }
    public int DestinationCityId { get; private set; }
    public decimal DistanceKm { get; private set; }
    public decimal Tariff { get; private set; }

    // Navigation
    public City? OriginCity { get; private set; }
    public City? DestinationCity { get; private set; }

    private Distance() { }

    public Distance(int originCityId, int destinationCityId, decimal distanceKm, decimal tariff)
    {
        if (distanceKm <= 0) throw new DomainException("Distance must be positive.");
        if (tariff < 0) throw new DomainException("Tariff cannot be negative.");

        OriginCityId = originCityId;
        DestinationCityId = destinationCityId;
        DistanceKm = distanceKm;
        Tariff = tariff;
    }
}
