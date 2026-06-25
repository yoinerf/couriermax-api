using CourierMax.Domain.Common;
using CourierMax.Domain.Enums;
using CourierMax.Domain.Exceptions;

namespace CourierMax.Domain.Entities;

public class ShipmentHistory
{
    public int Id { get; private set; }
    public int ShipmentId { get; private set; }
    public ShipmentStatus? PreviousStatus { get; private set; }
    public ShipmentStatus NewStatus { get; private set; }
    public string? Reason { get; private set; }
    public string ChangedBy { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    // Navegación
    public Shipment? Shipment { get; private set; }

    private ShipmentHistory() { }

    public ShipmentHistory(
        int shipmentId,
        ShipmentStatus? previousStatus,
        ShipmentStatus newStatus,
        string changedBy,
        string? reason = null,
        DateTime? createdAt = null)
    {
        if (string.IsNullOrWhiteSpace(changedBy))
            throw new DomainException("ChangedBy cannot be empty.");

        ShipmentId = shipmentId;
        PreviousStatus = previousStatus;
        NewStatus = newStatus;
        ChangedBy = changedBy;
        Reason = reason;
        CreatedAt = createdAt ?? SystemTime.Now();
    }
}
