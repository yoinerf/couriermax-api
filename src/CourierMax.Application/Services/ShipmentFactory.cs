using CourierMax.Application.DTOs;
using CourierMax.Domain.Entities;
using CourierMax.Domain.ValueObjects;

namespace CourierMax.Application.Services;

public class ShipmentFactory : IShipmentFactory
{
    public Shipment Create(CreateShipmentDto dto, int sequence, decimal totalCost, PackageDimensions dimensions, DateTime createdAt)
    {
        var trackingCode = TrackingCode.Generate(sequence);

        var shipment = new Shipment(
            trackingCode: trackingCode.Value,
            serviceType: dto.ServiceType,
            packageType: dto.PackageType,
            packageWeight: dto.PackageWeight,
            packageDimensions: dto.PackageDimensions,
            packageVolumeM3: dimensions.VolumeInM3,
            senderName: dto.SenderName,
            senderPhone: dto.SenderPhone,
            senderAddress: dto.SenderAddress,
            recipientName: dto.RecipientName,
            recipientPhone: dto.RecipientPhone,
            recipientAddress: dto.RecipientAddress,
            originCityId: dto.OriginCityId,
            destinationCityId: dto.DestinationCityId,
            createdAt: createdAt);

        shipment.SetTotalCost(totalCost);
        return shipment;
    }
}
