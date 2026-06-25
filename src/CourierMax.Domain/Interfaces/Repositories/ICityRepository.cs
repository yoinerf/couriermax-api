using CourierMax.Domain.Entities;

namespace CourierMax.Domain.Interfaces.Repositories;

public interface ICityRepository
{
    Task<City?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<City?> GetByNameAsync(string name, CancellationToken ct = default);
    Task<IReadOnlyList<City>> GetAllAsync(CancellationToken ct = default);
    Task<bool> ExistsAsync(int id, CancellationToken ct = default);
}
