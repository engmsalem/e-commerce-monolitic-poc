namespace ECommercePoc.Application.Exceptions;

/// <summary>
/// Thrown when a request conflicts with existing state (e.g. a duplicate unique key).
/// Caught by middleware → 409.
/// </summary>
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}
