namespace CourierMax.Domain.Interfaces;

/// <summary>
/// Servicio para calcular días hábiles excluyendo fines de semana y festivos colombianos.
/// </summary>
public interface IBusinessDayCalculator
{
    /// <summary>
    /// Calcula el número de días hábiles entre dos fechas (excluyendo la fecha de inicio).
    /// </summary>
    int CountBusinessDays(DateTime start, DateTime end);

    /// <summary>
    /// Agrega un número dado de días hábiles a la fecha de inicio.
    /// </summary>
    DateTime AddBusinessDays(DateTime start, int businessDays);

    /// <summary>
    /// Retorna true si la fecha dada es un día hábil (no es fin de semana ni festivo).
    /// </summary>
    bool IsBusinessDay(DateTime date);
}
