namespace ECommercePoc.Application.Exceptions;

/// <summary>
/// Thrown when a requested resource is not found. Caught by middleware → 404.
/// Separate from DomainException because "not found" is a query concern, not a business rule.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string resourceName, object key)
        : base($"{resourceName} with id '{key}' was not found.") { }
}
