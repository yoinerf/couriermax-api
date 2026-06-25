using CourierMax.Domain.Entities;

namespace CourierMax.Domain.Interfaces.Repositories;

public interface IDistanceRepository
{
    Task<Distance?> GetByRouteAsync(int originCityId, int destinationCityId, CancellationToken ct = default);
    Task<IReadOnlyList<Distance>> GetAllAsync(CancellationToken ct = default);
}
