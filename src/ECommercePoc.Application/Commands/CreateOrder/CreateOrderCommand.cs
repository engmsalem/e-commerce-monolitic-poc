using ECommercePoc.Application.Dtos;
using MediatR;

namespace ECommercePoc.Application.Commands.CreateOrder;

/// <summary>
/// Creates a new order for a customer with one or more line items.
/// </summary>
public sealed class CreateOrderCommand : IRequest<OrderDto>
{
    public Guid CustomerId { get; init; }
    public List<OrderLineItemRequest> Items { get; init; } = [];
}
