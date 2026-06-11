using ECommercePoc.Application.Dtos;
using ECommercePoc.Application.Exceptions;
using ECommercePoc.Application.Interfaces;
using MediatR;

namespace ECommercePoc.Application.Commands.CancelOrder;

public sealed class CancelOrderHandler : IRequestHandler<CancelOrderCommand, OrderDto>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CancelOrderHandler(IOrderRepository orderRepository, IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<OrderDto> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
            throw new NotFoundException("Order", request.OrderId);

        order.Cancel(); // Throws DomainException if already shipped or cancelled

        await _orderRepository.UpdateAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

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
