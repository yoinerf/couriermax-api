using CourierMax.Domain.Entities;

namespace CourierMax.Domain.Interfaces.Repositories;

public interface IDriverRepository
{
    Task<Driver?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<Driver>> GetActiveDriversAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Driver>> GetAllAsync(CancellationToken ct = default);
}
