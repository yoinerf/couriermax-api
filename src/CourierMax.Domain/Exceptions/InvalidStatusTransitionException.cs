namespace CourierMax.Domain.Exceptions;

public class InvalidStatusTransitionException : DomainException
{
    public InvalidStatusTransitionException(string from, string to)
        : base($"Cannot transition shipment from status '{from}' to '{to}'.", "INVALID_STATUS_TRANSITION")
    {
    }
}
