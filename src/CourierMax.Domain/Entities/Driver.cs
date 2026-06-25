using CourierMax.Domain.Common;
using CourierMax.Domain.Exceptions;

namespace CourierMax.Domain.Entities;

public class Driver
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Navegación
    public Vehicle? Vehicle { get; private set; }

    private Driver() { }

    public Driver(string name, DateTime? createdAt = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Driver name cannot be empty.");

        Name = name.Trim();
        IsActive = true;
        CreatedAt = createdAt ?? SystemTime.Now();
    }

    public void Deactivate() => IsActive = false;

    public void Activate() => IsActive = true;
}
