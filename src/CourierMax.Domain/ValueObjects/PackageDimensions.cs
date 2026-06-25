using CourierMax.Domain.Exceptions;

namespace CourierMax.Domain.ValueObjects;

/// <summary>
/// Representa las dimensiones del paquete en centímetros (Largo x Ancho x Alto).
/// Cada dimensión debe estar entre 1 y 200 cm.
/// </summary>
public sealed record PackageDimensions
{
    public decimal Length { get; }
    public decimal Width { get; }
    public decimal Height { get; }

    /// <summary>Volumen en centímetros cúbicos.</summary>
    public decimal VolumeInCm3 => Length * Width * Height;

    /// <summary>Volumen en metros cúbicos.</summary>
    public decimal VolumeInM3 => VolumeInCm3 / 1_000_000m;

    private PackageDimensions(decimal length, decimal width, decimal height)
    {
        Length = length;
        Width = width;
        Height = height;
    }

    public static PackageDimensions From(decimal length, decimal width, decimal height)
    {
        ValidateDimension(length, nameof(length));
        ValidateDimension(width, nameof(width));
        ValidateDimension(height, nameof(height));
        return new PackageDimensions(length, width, height);
    }

    /// <summary>Parsea desde el formato de cadena "LxWxH" (Largo x Ancho x Alto).</summary>
    public static PackageDimensions Parse(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Dimensions string cannot be empty.");

        var parts = value.Split('x', 'X');
        if (parts.Length != 3)
            throw new DomainException($"Invalid dimensions format: '{value}'. Expected 'LxWxH'.");

        if (!decimal.TryParse(parts[0].Trim(), out var l) ||
            !decimal.TryParse(parts[1].Trim(), out var w) ||
            !decimal.TryParse(parts[2].Trim(), out var h))
            throw new DomainException($"Dimensions must be numeric values: '{value}'.");

        return From(l, w, h);
    }

    private static void ValidateDimension(decimal value, string name)
    {
        if (value < 1 || value > 200)
            throw new DomainException($"Dimension '{name}' must be between 1 and 200 cm. Got: {value}.");
    }

    public override string ToString() => $"{Length}x{Width}x{Height}";
}
