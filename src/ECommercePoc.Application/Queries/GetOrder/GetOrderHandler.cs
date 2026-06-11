using ECommercePoc.Application.Dtos;
using ECommercePoc.Application.Exceptions;
using ECommercePoc.Application.Interfaces;
using MediatR;

namespace ECommercePoc.Application.Queries.GetOrder;

public sealed class GetOrderHandler : IRequestHandler<GetOrderQuery, OrderDto>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrderHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<OrderDto> Handle(GetOrderQuery request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
            throw new NotFoundException("Order", request.OrderId);

        return MapToDto(order);
    }

    private static OrderDto MapToDto(Domain.Entities.Order order) => new()
    {
        Id = order.Id,
        CustomerId = order.CustomerId,
        Status = order.Status.ToString(),
        CreatedDate = order.CreatedDate,
        Total = order.Total.Amount,
        Currency = order.Total.Currency,
        Items = order.Items.Select(i => new OrderItemDto
        {
            Id = i.Id,
            ProductId = i.ProductId,
            ProductName = i.ProductName,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice.Amount,
            Currency = i.UnitPrice.Currency
        }).ToList()
    };
}
