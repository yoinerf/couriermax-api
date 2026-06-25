using System.Text.RegularExpressions;
using CourierMax.Domain.Exceptions;

namespace CourierMax.Domain.ValueObjects;

public sealed record TrackingCode
{
    private static readonly Regex _pattern = new(@"^CM-\d{8}$", RegexOptions.Compiled);

    public string Value { get; }

    private TrackingCode(string value) => Value = value;

    public static TrackingCode From(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || !_pattern.IsMatch(value))
            throw new DomainException($"Invalid tracking code format: '{value}'. Must match CM-XXXXXXXX.");

        return new TrackingCode(value);
    }

    public static TrackingCode Generate(int sequence)
    {
        var code = $"CM-{sequence:D8}";
        return new TrackingCode(code);
    }

    public override string ToString() => Value;

    public static implicit operator string(TrackingCode tc) => tc.Value;
}
