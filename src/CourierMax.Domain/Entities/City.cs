using CourierMax.Domain.Exceptions;

namespace CourierMax.Domain.Entities;

public class City
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;

    private City() { }

    public City(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("City name cannot be empty.");

        Name = name.Trim();
    }
}
