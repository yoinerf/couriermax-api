using CourierMax.Domain.Interfaces;

namespace CourierMax.Infrastructure.Services;

/// <summary>
/// Calcula los días hábiles excluyendo los fines de semana y los festivos públicos colombianos para 2026.
/// Implementa RN-02.
/// </summary>
public class BusinessDayCalculator : IBusinessDayCalculator
{
    /// <summary>
    /// Festivos colombianos de 2026 según pruebaTecnica.md:
    /// 1 Ene, 26 Ene, 30 Ene, 24 Mar, 1 May, 1 Jun, 29 Jun, 20 Jul, 17 Ago, 20 Oct, 9 Nov, 8 Dec
    /// </summary>
    private static readonly HashSet<DateOnly> _colombianHolidays2026 =
    [
        new DateOnly(2026, 1,  1),   // 1 Ene
        new DateOnly(2026, 1,  26),  // 26 Ene
        new DateOnly(2026, 1,  30),  // 30 Ene
        new DateOnly(2026, 3,  24),  // 24 Mar
        new DateOnly(2026, 5,  1),   // 1 May
        new DateOnly(2026, 6,  1),   // 1 Jun
        new DateOnly(2026, 6,  29),  // 29 Jun
        new DateOnly(2026, 7,  20),  // 20 Jul
        new DateOnly(2026, 8,  17),  // 17 Ago
        new DateOnly(2026, 10, 20),  // 20 Oct
        new DateOnly(2026, 11, 9),   // 9 Nov
        new DateOnly(2026, 12, 8),   // 8 Dec
    ];

    /// <inheritdoc/>
    public bool IsBusinessDay(DateTime date)
    {
        var d = DateOnly.FromDateTime(date);
        return date.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday)
               && !_colombianHolidays2026.Contains(d);
    }

    /// <inheritdoc/>
    public int CountBusinessDays(DateTime start, DateTime end)
    {
        if (end <= start) return 0;

        var count = 0;
        var current = start.Date.AddDays(1); // excluyendo la fecha de inicio

        while (current <= end.Date)
        {
            if (IsBusinessDay(current)) count++;
            current = current.AddDays(1);
        }

        return count;
    }

    /// <inheritdoc/>
    public DateTime AddBusinessDays(DateTime start, int businessDays)
    {
        if (businessDays == 0) return start;

        var current = start.Date;
        var added = 0;

        while (added < businessDays)
        {
            current = current.AddDays(1);
            if (IsBusinessDay(current)) added++;
        }

        return current;
    }
}
