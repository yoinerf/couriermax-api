using CourierMax.Domain.Enums;
using FluentValidation;
using CourierMax.Application.DTOs;
using System.Text.RegularExpressions;

namespace CourierMax.Application.Validators;

public class CreateShipmentValidator : AbstractValidator<CreateShipmentDto>
{
    private static readonly Regex _phoneRegex = new(@"^[36]\d{9}$", RegexOptions.Compiled);
    private static readonly Regex _dimensionsRegex = new(@"^\d+(\.\d+)?[xX]\d+(\.\d+)?[xX]\d+(\.\d+)?$", RegexOptions.Compiled);

    public CreateShipmentValidator()
    {
        // Remitente
        RuleFor(x => x.SenderName)
            .NotEmpty().WithMessage("Sender name is required.")
            .MaximumLength(100).WithMessage("Sender name cannot exceed 100 characters.");

        RuleFor(x => x.SenderPhone)
            .NotEmpty().WithMessage("Sender phone is required.")
            .Matches(_phoneRegex).WithMessage("Sender phone must be a valid Colombian number (10 digits starting with 3 or 6).");

        RuleFor(x => x.SenderAddress)
            .NotEmpty().WithMessage("Sender address is required.")
            .MaximumLength(255).WithMessage("Sender address cannot exceed 255 characters.");

        // Destinatario
        RuleFor(x => x.RecipientName)
            .NotEmpty().WithMessage("Recipient name is required.")
            .MaximumLength(100).WithMessage("Recipient name cannot exceed 100 characters.");

        RuleFor(x => x.RecipientPhone)
            .NotEmpty().WithMessage("Recipient phone is required.")
            .Matches(_phoneRegex).WithMessage("Recipient phone must be a valid Colombian number (10 digits starting with 3 or 6).");

        RuleFor(x => x.RecipientAddress)
            .NotEmpty().WithMessage("Recipient address is required.")
            .MaximumLength(255).WithMessage("Recipient address cannot exceed 255 characters.");

        // Paquete
        RuleFor(x => x.PackageWeight)
            .InclusiveBetween(0.1m, 100m)
            .WithMessage("Package weight must be between 0.1 kg and 100 kg.");

        RuleFor(x => x.PackageDimensions)
            .NotEmpty().WithMessage("Package dimensions are required.")
            .Matches(_dimensionsRegex).WithMessage("Package dimensions must be in format 'LxWxH' (e.g., 30x20x15).")
            .Must(BeValidDimensionRanges).WithMessage("Each dimension must be between 1 and 200 cm.");

        // Ciudades
        RuleFor(x => x.OriginCityId)
            .GreaterThan(0).WithMessage("Origin city is required.");

        RuleFor(x => x.DestinationCityId)
            .GreaterThan(0).WithMessage("Destination city is required.")
            .NotEqual(x => x.OriginCityId).WithMessage("Destination city must be different from origin city.");

        RuleFor(x => x.ServiceType)
            .IsInEnum().WithMessage("Invalid service type.");

        RuleFor(x => x.PackageType)
            .IsInEnum().WithMessage("Invalid package type.");
    }

    private static bool BeValidDimensionRanges(string dimensions)
    {
        if (string.IsNullOrWhiteSpace(dimensions)) return false;
        var parts = dimensions.Split('x', 'X');
        if (parts.Length != 3) return false;

        foreach (var part in parts)
        {
            if (!decimal.TryParse(part.Trim(), out var val)) return false;
            if (val < 1 || val > 200) return false;
        }

        return true;
    }
}

public class UpdateShipmentStatusValidator : AbstractValidator<UpdateShipmentStatusDto>
{
    public UpdateShipmentStatusValidator()
    {
        RuleFor(x => x.NewStatus)
            .IsInEnum().WithMessage("Invalid shipment status.");

        RuleFor(x => x.ChangedBy)
            .NotEmpty().WithMessage("ChangedBy is required.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Cancellation reason is required.")
            .MinimumLength(5).WithMessage("Cancellation reason must have at least 5 characters.")
            .When(x => x.NewStatus == ShipmentStatus.Cancelled);
    }
}

public class AssignShipmentValidator : AbstractValidator<AssignShipmentDto>
{
    public AssignShipmentValidator()
    {
        RuleFor(x => x.VehicleId)
            .GreaterThan(0).When(x => x.VehicleId.HasValue).WithMessage("A valid vehicle ID is required if provided.");

        RuleFor(x => x.AssignedBy)
            .NotEmpty().WithMessage("AssignedBy is required.");
    }
}
