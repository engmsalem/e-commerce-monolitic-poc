using ECommercePoc.Application.Dtos;
using MediatR;

namespace ECommercePoc.Application.Commands.CancelOrder;

/// <summary>
/// Cancels an existing order. Fails with DomainException if the order is already shipped.
/// </summary>
public sealed class CancelOrderCommand : IRequest<OrderDto>
{
    public Guid OrderId { get; init; }
}
