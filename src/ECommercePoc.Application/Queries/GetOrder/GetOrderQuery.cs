using ECommercePoc.Application.Dtos;
using MediatR;

namespace ECommercePoc.Application.Queries.GetOrder;

/// <summary>
/// Retrieves a single order by ID, including line items.
/// </summary>
public sealed class GetOrderQuery : IRequest<OrderDto>
{
    public Guid OrderId { get; init; }
}
