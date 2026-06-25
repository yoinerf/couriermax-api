using System;
using CourierMax.Application.DTOs;
using CourierMax.Domain.Entities;
using CourierMax.Domain.ValueObjects;

namespace CourierMax.Application.Services;

public interface IShipmentFactory
{
    Shipment Create(CreateShipmentDto dto, int sequence, decimal totalCost, PackageDimensions dimensions, DateTime createdAt);
}
