using CourierMax.Application.Common;
using CourierMax.Application.DTOs;
using CourierMax.Domain.Entities;
using CourierMax.Domain.Enums;
using CourierMax.Domain.Interfaces;
using CourierMax.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace CourierMax.Application.Services;

public class ReportService : IReportService
{
    private readonly IDriverRepository _drivers;
    private readonly IShipmentRepository _shipments;
    private readonly IVehicleRepository _vehicles;
    private readonly IBusinessDayCalculator _businessDays;
    private readonly ILogger<ReportService> _logger;

    public ReportService(
        IDriverRepository drivers,
        IShipmentRepository shipments,
        IVehicleRepository vehicles,
        IBusinessDayCalculator businessDays,
        ILogger<ReportService> logger)
    {
        _drivers = drivers;
        _shipments = shipments;
        _vehicles = vehicles;
        _businessDays = businessDays;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<DriverPerformanceDto>>> GetAllDriverPerformanceAsync(CancellationToken ct = default)
    {
        var drivers = await _drivers.GetAllAsync(ct);
        if (drivers.Count == 0)
            return Result<IReadOnlyList<DriverPerformanceDto>>.Success(Array.Empty<DriverPerformanceDto>());

        var driverIds = drivers.Select(d => d.Id).ToHashSet();
        var shipments = await _shipments.GetByDriverIdsAsync(driverIds, ct);
        var shipmentsByDriver = shipments
            .GroupBy(s => s.Vehicle!.DriverId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        var result = drivers.Select(driver =>
        {
            shipmentsByDriver.TryGetValue(driver.Id, out var driverShipments);
            return BuildPerformance(driver.Id, driver.Name, driverShipments ?? new List<Shipment>());
        }).ToList();

        return Result<IReadOnlyList<DriverPerformanceDto>>.Success(result);
    }

    public async Task<Result<DriverPerformanceDto>> GetDriverPerformanceAsync(int driverId, CancellationToken ct = default)
    {
        var driver = await _drivers.GetByIdAsync(driverId, ct);
        if (driver is null)
            return Result<DriverPerformanceDto>.Failure($"Driver with ID {driverId} not found.", ErrorCodes.DriverNotFound);

        var shipments = await _shipments.GetByDriverIdAsync(driverId, ct);
        var perf = BuildPerformance(driver.Id, driver.Name, shipments);
        return Result<DriverPerformanceDto>.Success(perf);
    }

    private DriverPerformanceDto BuildPerformance(int driverId, string driverName, IReadOnlyCollection<Shipment> shipments)
    {
        var delivered = shipments.Where(s => s.Status == ShipmentStatus.Delivered).ToList();
        var cancelled = shipments.Count(s => s.Status == ShipmentStatus.Cancelled);
        var inTransit = shipments.Count(s => s.Status == ShipmentStatus.InTransit ||
                                              s.Status == ShipmentStatus.Assigned ||
                                              s.Status == ShipmentStatus.Created);

        // Tiempo promedio de entrega en días hábiles (desde AssignedAt hasta DeliveredAt)
        var deliveryTimes = delivered
            .Where(s => s.AssignedAt.HasValue && s.DeliveredAt.HasValue)
            .Select(s => (double)_businessDays.CountBusinessDays(s.AssignedAt!.Value, s.DeliveredAt!.Value))
            .ToList();

        var avgDeliveryDays = deliveryTimes.Count > 0 ? deliveryTimes.Average() : 0.0;

        // Cumplimiento de SLA: entregado dentro de los días hábiles de SLA desde CreatedAt
        var withinSla = delivered.Count(s =>
        {
            if (!s.DeliveredAt.HasValue) return false;
            var slaLimit = SlaConfiguration.ServiceLevels[s.ServiceType];
            var elapsed = _businessDays.CountBusinessDays(s.CreatedAt, s.DeliveredAt.Value);
            return elapsed <= slaLimit;
        });

        var slaPercent = delivered.Count > 0
            ? Math.Round((double)withinSla / delivered.Count * 100, 2)
            : 100.0;

        var totalWeight = delivered.Sum(s => s.PackageWeight);

        return new DriverPerformanceDto(
            DriverId: driverId,
            DriverName: driverName,
            TotalAssigned: shipments.Count,
            TotalDelivered: delivered.Count,
            TotalCancelled: cancelled,
            TotalInTransit: inTransit,
            AverageDeliveryDays: Math.Round(avgDeliveryDays, 2),
            SlaCompliancePercent: slaPercent,
            TotalWeightTransported: totalWeight
        );
    }
}
