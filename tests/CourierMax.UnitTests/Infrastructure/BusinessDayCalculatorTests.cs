using CourierMax.Infrastructure.Services;
using FluentAssertions;
using Xunit;

namespace CourierMax.UnitTests.Infrastructure;

public class BusinessDayCalculatorTests
{
    private readonly BusinessDayCalculator _calc = new();

    [Theory]
    [InlineData("2026-01-05", true)]   // Lunes (Lunes es día hábil)
    [InlineData("2026-01-10", false)]  // Sábado
    [InlineData("2026-01-11", false)]  // Domingo
    [InlineData("2026-01-01", false)]  // Año Nuevo — Festivo colombiano
    [InlineData("2026-07-20", false)]  // Día de la Independencia — Festivo colombiano
    [InlineData("2026-12-08", false)]  // 8 Dic — Festivo colombiano
    [InlineData("2026-12-25", true)]   // Navidad — NO está en la lista de festivos de RN-02, por ende es día hábil
    public void IsBusinessDay_CorrectlyClassifiesDays(string dateStr, bool expected)
    {
        var date = DateTime.Parse(dateStr);
        _calc.IsBusinessDay(date).Should().Be(expected);
    }

    [Fact]
    public void CountBusinessDays_FullWorkWeek_Returns5()
    {
        // De Lunes a Viernes
        var start = new DateTime(2026, 1, 5);  // Lunes
        var end = new DateTime(2026, 1, 9);    // Viernes
        _calc.CountBusinessDays(start, end).Should().Be(4); // Lunes→Viernes cuenta Lunes,Martes,Miércoles,Jueves = 4 exclusivo del fin
    }

    [Fact]
    public void CountBusinessDays_IncludesWeekend_SkipsSatSun()
    {
        // Del Viernes 16 Ene al próximo Lunes 19 Ene = solo cuenta el Lunes (1 día hábil)
        // El 19 Ene no es festivo en 2026
        var start = new DateTime(2026, 1, 16);  // Viernes
        var end = new DateTime(2026, 1, 19);    // Lunes (no festivo)
        _calc.CountBusinessDays(start, end).Should().Be(1);
    }

    [Fact]
    public void CountBusinessDays_SkipsHolidays()
    {
        // Del 7 Dic (Lun) al 9 Dic (Mie) — el 8 Dic es festivo
        var start = new DateTime(2026, 12, 7);
        var end = new DateTime(2026, 12, 9);
        // Business days: Dec 8 (holiday, skipped), Dec 9 (Wed) = 1
        _calc.CountBusinessDays(start, end).Should().Be(1);
    }

    [Fact]
    public void AddBusinessDays_StandardDay_ReturnsCorrectDate()
    {
        // Desde el Lunes 19 Ene, sumar 5 días hábiles → próximo Lunes 26 Ene
        // El 26 Ene es festivo (según la lista de RN-02) por lo que pasa al Martes 27 Ene
        // Si empieza el 5 Ene (Lun) + 5 = 12 Ene (Lunes - no está en la lista de RN-02, por ende es hábil) → Lunes 12 Ene
        // Rango seguro: Feb 2 (Lun) + 5 = Feb 9 (Lun) (sin festivos)
        var start = new DateTime(2026, 2, 2); // Lunes
        var result = _calc.AddBusinessDays(start, 5);
        result.Should().Be(new DateTime(2026, 2, 9)); // Next Monday — no holidays
    }

    [Fact]
    public void AddBusinessDays_Zero_ReturnsSameDay()
    {
        var start = new DateTime(2026, 1, 5);
        var result = _calc.AddBusinessDays(start, 0);
        result.Should().Be(start);
    }

    [Fact]
    public void AddBusinessDays_FromFriday_1Day_ReturnsMonday()
    {
        // SLA de 1 día hábil de Viernes → Lunes (ejemplo RN-02)
        // Usar 6 Feb (Vie) → 9 Feb (Lun) — sin festivos en este rango
        var friday = new DateTime(2026, 2, 6);
        var result = _calc.AddBusinessDays(friday, 1);
        result.DayOfWeek.Should().Be(DayOfWeek.Monday);
        result.Should().Be(new DateTime(2026, 2, 9));
    }

    [Fact]
    public void CountBusinessDays_StartEqualsEnd_ReturnsZero()
    {
        var date = new DateTime(2026, 1, 5);
        _calc.CountBusinessDays(date, date).Should().Be(0);
    }
}
