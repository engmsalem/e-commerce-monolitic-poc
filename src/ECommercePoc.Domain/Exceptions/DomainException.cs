namespace ECommercePoc.Domain.Exceptions;

/// <summary>
/// Represents a business rule violation in the domain layer.
/// Caught by global error middleware and mapped to 400 Bad Request.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }

    public DomainException(string message, Exception innerException)
        : base(message, innerException) { }
}
