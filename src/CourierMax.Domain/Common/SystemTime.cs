namespace CourierMax.Domain.Common;

/// <summary>
/// Provee la hora actual del sistema estandarizada a la zona horaria de Colombia (UTC-5).
/// </summary>
public static class SystemTime
{
    /// <summary>
    /// Retorna la hora actual en Colombia (UTC-5).
    /// </summary>
    public static DateTime Now()
    {
        return new DateTimeOffset(DateTime.UtcNow).ToOffset(TimeSpan.FromHours(-5)).DateTime;
    }
}
