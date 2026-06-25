using CourierMax.Application.Common;
using CourierMax.Application.DTOs;
using CourierMax.Application.Mappings;
using CourierMax.Application.Services.Tariff;
using CourierMax.Domain.Entities;
using CourierMax.Domain.Exceptions;
using CourierMax.Domain.Enums;
using CourierMax.Domain.Interfaces;
using DomainCommon = CourierMax.Domain.Common;
using CourierMax.Domain.Interfaces.Repositories;
using CourierMax.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace CourierMax.Application.Services;

public class ShipmentService : IShipmentService
{
    private readonly IShipmentRepository _shipments;
    private readonly IVehicleRepository _vehicles;
    private readonly IDriverRepository _drivers;
    private readonly ICityRepository _cities;
    private readonly IDistanceRepository _distances;
    private readonly IUnitOfWork _uow;
    private readonly IShipmentFactory _shipmentFactory;
    private readonly ITariffCalculator _tariff;
    private readonly IBusinessDayCalculator _businessDays;
    private readonly DomainCommon.ISystemTime _systemTime;
    private readonly ILogger<ShipmentService> _logger;

    // Días hábiles de SLA por tipo de servicio
    public ShipmentService(
        IShipmentRepository shipments,
        IVehicleRepository vehicles,
        IDriverRepository drivers,
        ICityRepository cities,
        IDistanceRepository distances,
        IUnitOfWork uow,
        IShipmentFactory shipmentFactory,
        ITariffCalculator tariff,
        IBusinessDayCalculator businessDays,
        DomainCommon.ISystemTime systemTime,
        ILogger<ShipmentService> logger)
    {
        _shipments = shipments;
        _vehicles = vehicles;
        _drivers = drivers;
        _cities = cities;
        _distances = distances;
        _uow = uow;
        _shipmentFactory = shipmentFactory;
        _tariff = tariff;
        _businessDays = businessDays;
        _systemTime = systemTime;
        _logger = logger;
    }

    // RF-01: Crear envío
    public async Task<Result<ShipmentResponseDto>> CreateAsync(CreateShipmentDto dto, CancellationToken ct = default)
    {
        // Validar ciudades
        if (!await _cities.ExistsAsync(dto.OriginCityId, ct))
            return Result<ShipmentResponseDto>.Failure($"Origin city with ID {dto.OriginCityId} not found.", ErrorCodes.CityNotFound);

        if (!await _cities.ExistsAsync(dto.DestinationCityId, ct))
            return Result<ShipmentResponseDto>.Failure($"Destination city with ID {dto.DestinationCityId} not found.", ErrorCodes.CityNotFound);

        // Validar que exista la distancia de la ruta
        var distance = await _distances.GetByRouteAsync(dto.OriginCityId, dto.DestinationCityId, ct);
        if (distance is null)
            return Result<ShipmentResponseDto>.Failure(
                $"No route configured between cities {dto.OriginCityId} and {dto.DestinationCityId}.",
                ErrorCodes.RouteNotFound);

        // Parsear y validar dimensiones
        PackageDimensions dimensions;
        try
        {
            dimensions = PackageDimensions.Parse(dto.PackageDimensions);
        }
        catch (DomainException ex)
        {
            return Result<ShipmentResponseDto>.Failure(ex.Message, ErrorCodes.InvalidDimensions);
        }

        // Generar código de rastreo (tracking code)
        var sequence = await _shipments.GetNextSequenceAsync(ct);
        var trackingCode = TrackingCode.Generate(sequence);

        // validación de seguridad para garantizar unicidad
        if (await _shipments.ExistsByTrackingCodeAsync(trackingCode.Value, ct))
            return Result<ShipmentResponseDto>.Failure("Could not generate a unique tracking code. Please retry.", ErrorCodes.TrackingCodeConflict);

        // Calcular tarifa
        var cost = _tariff.Calculate(dto.ServiceType, dto.PackageType, dto.PackageWeight, distance.Tariff);

        // Construir la entidad usando la fábrica
        var shipment = _shipmentFactory.Create(dto, sequence, cost, dimensions, _systemTime.Now());


        await _shipments.AddAsync(shipment, ct);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Shipment {TrackingCode} created with ID {Id}", trackingCode.Value, shipment.Id);

        var created = await _shipments.GetByIdWithDetailsAsync(shipment.Id, ct);
        return Result<ShipmentResponseDto>.Success(created!.ToResponseDto());
    }

    // Consulta: Obtener por ID
    public async Task<Result<ShipmentResponseDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var shipment = await _shipments.GetByIdWithDetailsAsync(id, ct);
        if (shipment is null)
            return Result<ShipmentResponseDto>.Failure($"Shipment with ID {id} not found.", ErrorCodes.ShipmentNotFound);

        return Result<ShipmentResponseDto>.Success(shipment.ToResponseDto());
    }

    // Consulta: Obtener por código de rastreo
    public async Task<Result<ShipmentResponseDto>> GetByTrackingCodeAsync(string trackingCode, CancellationToken ct = default)
    {
        var shipment = await _shipments.GetByTrackingCodeAsync(trackingCode, ct);
        if (shipment is null)
            return Result<ShipmentResponseDto>.Failure($"Shipment with tracking code '{trackingCode}' not found.", ErrorCodes.ShipmentNotFound);

        return Result<ShipmentResponseDto>.Success(shipment.ToResponseDto());
    }

    // RF-02: Actualizar estado
    public async Task<Result<ShipmentResponseDto>> UpdateStatusAsync(int id, UpdateShipmentStatusDto dto, CancellationToken ct = default)
    {
        var shipment = await _shipments.GetByIdWithDetailsAsync(id, ct);
        if (shipment is null)
            return Result<ShipmentResponseDto>.Failure($"Shipment with ID {id} not found.", ErrorCodes.ShipmentNotFound);

        try
        {
            shipment.TransitionTo(dto.NewStatus, dto.ChangedBy, dto.Reason, _systemTime.Now());

            // Si se cancela, liberar la capacidad del vehículo
            if (dto.NewStatus == ShipmentStatus.Cancelled && shipment.VehicleId.HasValue)
            {
                shipment.UnassignVehicle();
                _logger.LogInformation("Shipment {Id} cancelled — vehicle capacity released.", id);
            }

            _shipments.Update(shipment);
            await _uow.SaveChangesAsync(ct);

            var updated = await _shipments.GetByIdWithDetailsAsync(id, ct);
            return Result<ShipmentResponseDto>.Success(updated!.ToResponseDto());
        }
        catch (Domain.Exceptions.DomainException ex)
        {
            return Result<ShipmentResponseDto>.Failure(ex.Message, ex.Code);
        }
    }

    // RF-03: Asignar a vehículo
    public async Task<Result<ShipmentResponseDto>> AssignToVehicleAsync(int id, AssignShipmentDto dto, CancellationToken ct = default)
    {
        var shipment = await _shipments.GetByIdWithDetailsAsync(id, ct);
        if (shipment is null)
            return Result<ShipmentResponseDto>.Failure($"Shipment with ID {id} not found.", ErrorCodes.ShipmentNotFound);

        if (shipment.Status != ShipmentStatus.Created)
            return Result<ShipmentResponseDto>.Failure(
                $"Cannot assign a shipment in status '{shipment.Status}'. Only 'Created' shipments can be assigned.",
                ErrorCodes.InvalidStatusForAssignment);

        Vehicle? vehicle = null;

        if (dto.VehicleId.HasValue)
        {
            // Asignación manual a un vehículo específico
            vehicle = await _vehicles.GetByIdWithShipmentsAsync(dto.VehicleId.Value, ct);
            if (vehicle is null)
                return Result<ShipmentResponseDto>.Failure($"Vehicle with ID {dto.VehicleId} not found.", ErrorCodes.VehicleNotFound);

            if (!vehicle.IsAvailable)
                return Result<ShipmentResponseDto>.Failure($"Vehicle '{vehicle.Plate}' is not available.", ErrorCodes.VehicleNotAvailable);

            if (!vehicle.DriverId.HasValue)
                return Result<ShipmentResponseDto>.Failure($"Vehicle '{vehicle.Plate}' has no assigned driver.", ErrorCodes.VehicleNoDriver);

            var driver = await _drivers.GetByIdAsync(vehicle.DriverId.Value, ct);
            if (driver is null || !driver.IsActive)
                return Result<ShipmentResponseDto>.Failure(
                    $"Driver assigned to vehicle '{vehicle.Plate}' is not active.",
                    ErrorCodes.DriverNotActive);

            // Validación de capacidad (RN-01)
            try
            {
                vehicle.EnsureCanAccommodate(shipment.PackageWeight, shipment.PackageVolumeM3);
            }
            catch (VehicleCapacityExceededException ex)
            {
                return Result<ShipmentResponseDto>.Failure(ex.Message, ex.Code);
            }
        }
        else
        {
            // Auto-asignación con balanceo de carga
            var allVehicles = await _vehicles.GetAllWithShipmentsAsync(ct);
            
            // Filtramos vehículos disponibles con conductor activo y capacidad suficiente
            var candidates = allVehicles
                .Where(v => v.IsAvailable && v.DriverId.HasValue)
                .Where(v => v.Driver != null && v.Driver.IsActive)
                .Where(v => v.AvailableWeight >= shipment.PackageWeight && v.AvailableVolume >= shipment.PackageVolumeM3)
                .ToList();

            if (!candidates.Any())
                return Result<ShipmentResponseDto>.Failure(
                    "No available vehicles with enough capacity and active drivers to auto-assign this shipment.",
                ErrorCodes.NoCapacityAvailable);
            vehicle = candidates.OrderBy(v => v.CurrentWeightLoad).First();
        }

        // Recalcular tarifa con distancia
        var distance = await _distances.GetByRouteAsync(shipment.OriginCityId, shipment.DestinationCityId, ct);
        var cost = _tariff.Calculate(shipment.ServiceType, shipment.PackageType, shipment.PackageWeight, distance?.Tariff ?? 0m);

        shipment.AssignToVehicle(vehicle.Id, cost, _systemTime.Now());
        shipment.TransitionTo(ShipmentStatus.Assigned, dto.AssignedBy, $"Assigned to vehicle {vehicle.Plate}", _systemTime.Now());

        _shipments.Update(shipment);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Shipment {Id} assigned to vehicle {Plate}", id, vehicle.Plate);

        var updated = await _shipments.GetByIdWithDetailsAsync(id, ct);
        return Result<ShipmentResponseDto>.Success(updated!.ToResponseDto());
    }

     
    // RF-05: Obtener envíos atrasados (excedidos de SLA)
    public async Task<Result<IReadOnlyList<OverdueShipmentDto>>> GetOverdueAsync(
        DateTime? from, DateTime? to, CancellationToken ct = default)
    {
        var effectiveFrom = from ?? _systemTime.Now().AddDays(-365);
        var effectiveTo = to ?? _systemTime.Now();

        var shipments = await _shipments.GetOverdueAsync(effectiveFrom, effectiveTo, ct);
        var now = _systemTime.Now();

        var overdue = shipments
            .Where(s => s.Status is ShipmentStatus.Created or ShipmentStatus.Assigned or ShipmentStatus.InTransit)
            .Select(s =>
            {
                var sla = SlaConfiguration.ServiceLevels[s.ServiceType];
                var elapsed = _businessDays.CountBusinessDays(s.CreatedAt, now);
                var overdueBy = elapsed - sla;
                return (s, sla, elapsed, overdueBy);
            })
            .Where(x => x.overdueBy > 0)
            .Select(x => new OverdueShipmentDto(
                Id: x.s.Id,
                TrackingCode: x.s.TrackingCode,
                ServiceType: x.s.ServiceType.ToString(),
                SlaBusinessDays: x.sla,
                ElapsedBusinessDays: x.elapsed,
                OverdueByDays: x.overdueBy,
                CreatedAt: x.s.CreatedAt,
                OriginCity: x.s.OriginCity?.Name ?? string.Empty,
                DestinationCity: x.s.DestinationCity?.Name ?? string.Empty
            ))
            .ToList();

        return Result<IReadOnlyList<OverdueShipmentDto>>.Success(overdue);
    }
}
