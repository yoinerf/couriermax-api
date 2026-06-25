using CourierMax.Domain.Enums;

namespace CourierMax.Application.Common;

public static class SlaConfiguration
{
    public static readonly IReadOnlyDictionary<ServiceType, int> ServiceLevels = new Dictionary<ServiceType, int>
    {
        { ServiceType.Standard, 5 },
        { ServiceType.Express, 2 },
        { ServiceType.SameDay, 0 }
    };
}
