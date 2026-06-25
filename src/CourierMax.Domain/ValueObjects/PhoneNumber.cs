using System.Text.RegularExpressions;
using CourierMax.Domain.Exceptions;

namespace CourierMax.Domain.ValueObjects;

/// <summary>
/// Representa un número de teléfono colombiano: 10 dígitos que comienzan con 3 o 6.
/// </summary>
public sealed record PhoneNumber
{
    private static readonly Regex _pattern = new(@"^[36]\d{9}$", RegexOptions.Compiled);

    public string Value { get; }

    private PhoneNumber(string value) => Value = value;

    public static PhoneNumber From(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Phone number cannot be empty.");

        var cleaned = value.Trim().Replace(" ", "").Replace("-", "");

        if (!_pattern.IsMatch(cleaned))
            throw new DomainException(
                $"Invalid phone number format: '{value}'. Must be exactly 10 digits starting with 3 or 6.");

        return new PhoneNumber(cleaned);
    }

    public override string ToString() => Value;

    public static implicit operator string(PhoneNumber p) => p.Value;
}
