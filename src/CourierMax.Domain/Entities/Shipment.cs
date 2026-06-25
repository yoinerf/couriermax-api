using CourierMax.Domain.Common;
using CourierMax.Domain.Enums;
using CourierMax.Domain.Exceptions;

namespace CourierMax.Domain.Entities;

/// <summary>
/// Raíz de agregado central para el ciclo de vida del envío.
/// Encapsula todas las transiciones de estado y reglas de negocio.
/// </summary>
public class Shipment
{
    public int Id { get; private set; }
    public string TrackingCode { get; private set; } = string.Empty;
    public ShipmentStatus Status { get; private set; }
    public ServiceType ServiceType { get; private set; }
    public PackageType PackageType { get; private set; }

    // Paquete
    public decimal PackageWeight { get; private set; }       // kg
    public string PackageDimensions { get; private set; } = string.Empty; // LxWxH
    public decimal PackageVolumeM3 { get; private set; }    // calculado

    // Remitente
    public string SenderName { get; private set; } = string.Empty;
    public string SenderPhone { get; private set; } = string.Empty;
    public string SenderAddress { get; private set; } = string.Empty;

    // Destinatario
    public string RecipientName { get; private set; } = string.Empty;
    public string RecipientPhone { get; private set; } = string.Empty;
    public string RecipientAddress { get; private set; } = string.Empty;

    // Ruta
    public int OriginCityId { get; private set; }
    public int DestinationCityId { get; private set; }

    // Asignación
    public int? VehicleId { get; private set; }
    public decimal? TotalCost { get; private set; }
    public DateTime? AssignedAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Navegación
    public City? OriginCity { get; private set; }
    public City? DestinationCity { get; private set; }
    public Vehicle? Vehicle { get; private set; }
    public IReadOnlyCollection<ShipmentHistory> History => _history.AsReadOnly();
    private readonly List<ShipmentHistory> _history = [];

    private Shipment() { }

    public Shipment(
        string trackingCode,
        ServiceType serviceType,
        PackageType packageType,
        decimal packageWeight,
        string packageDimensions,
        decimal packageVolumeM3,
        string senderName,
        string senderPhone,
        string senderAddress,
        string recipientName,
        string recipientPhone,
        string recipientAddress,
        int originCityId,
        int destinationCityId,
        DateTime? createdAt = null)
    {
        TrackingCode = trackingCode;
        ServiceType = serviceType;
        PackageType = packageType;
        PackageWeight = packageWeight;
        PackageDimensions = packageDimensions;
        PackageVolumeM3 = packageVolumeM3;
        SenderName = senderName;
        SenderPhone = senderPhone;
        SenderAddress = senderAddress;
        RecipientName = recipientName;
        RecipientPhone = recipientPhone;
        RecipientAddress = recipientAddress;
        OriginCityId = originCityId;
        DestinationCityId = destinationCityId;
        Status = ShipmentStatus.Created;
        CreatedAt = createdAt ?? SystemTime.Now();
    }

    /// <summary>
    /// Transiciona este envío a un nuevo estado, aplicando el flujo permitido.
    /// CREADO → ASIGNADO → EN_TRANSITO → ENTREGADO
    /// CANCELADO (desde cualquier estado excepto ENTREGADO)
    /// </summary>
    public ShipmentHistory TransitionTo(ShipmentStatus newStatus, string changedBy, string? reason = null, DateTime? now = null)
    {
        ValidateTransition(newStatus, reason);

        var executedAt = now ?? SystemTime.Now();
        var previous = Status;
        Status = newStatus;

        if (newStatus == ShipmentStatus.Delivered)
            DeliveredAt = executedAt;

        var entry = new ShipmentHistory(Id, previous, newStatus, changedBy, reason, executedAt);
        _history.Add(entry);
        return entry;
    }

    private void ValidateTransition(ShipmentStatus newStatus, string? reason)
    {
        // No se puede cancelar un envío entregado
        if (newStatus == ShipmentStatus.Cancelled && Status == ShipmentStatus.Delivered)
            throw new InvalidStatusTransitionException(Status.ToString(), newStatus.ToString());

        // La cancelación requiere un motivo obligatorio (mínimo 5 caracteres)
        if (newStatus == ShipmentStatus.Cancelled)
        {
            if (string.IsNullOrWhiteSpace(reason) || reason.Trim().Length < 5)
                throw new DomainException(
                    "Cancellation requires a reason with at least 5 characters.",
                    "CANCELLATION_REASON_REQUIRED");
        }

        // Transiciones permitidas
        var allowed = (Status, newStatus) switch
        {
            (ShipmentStatus.Created, ShipmentStatus.Assigned) => true,
            (ShipmentStatus.Assigned, ShipmentStatus.InTransit) => true,
            (ShipmentStatus.InTransit, ShipmentStatus.Delivered) => true,
            (_, ShipmentStatus.Cancelled) when Status != ShipmentStatus.Delivered => true,
            _ => false
        };

        if (!allowed)
            throw new InvalidStatusTransitionException(Status.ToString(), newStatus.ToString());
    }

    public void AssignToVehicle(int vehicleId, decimal totalCost, DateTime? now = null)
    {
        VehicleId = vehicleId;
        TotalCost = totalCost;
        AssignedAt = now ?? SystemTime.Now();
    }

    public void UnassignVehicle()
    {
        VehicleId = null;
        AssignedAt = null;
    }

    public void SetTotalCost(decimal cost) => TotalCost = cost;
}
