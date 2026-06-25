namespace CourierMax.Domain.Common;

public sealed class SystemClock : ISystemTime
{
    public DateTime Now()
        => new DateTimeOffset(DateTime.UtcNow).ToOffset(TimeSpan.FromHours(-5)).DateTime;
}
