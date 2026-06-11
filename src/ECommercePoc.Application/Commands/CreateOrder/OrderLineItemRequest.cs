namespace ECommercePoc.Application.Commands.CreateOrder;

/// <summary>
/// A line item within a create-order request.
/// </summary>
public sealed class OrderLineItemRequest
{
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
}
