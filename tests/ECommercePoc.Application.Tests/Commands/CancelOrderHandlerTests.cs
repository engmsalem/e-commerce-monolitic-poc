using ECommercePoc.Application.Commands.CancelOrder;
using ECommercePoc.Application.Exceptions;
using ECommercePoc.Application.Interfaces;
using ECommercePoc.Domain.Entities;
using ECommercePoc.Domain.Enums;
using ECommercePoc.Domain.Exceptions;
using Moq;
using Xunit;

namespace ECommercePoc.Application.Tests.Commands;

public class CancelOrderHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly CancelOrderHandler _handler;

    public CancelOrderHandlerTests()
    {
        _handler = new CancelOrderHandler(_orderRepoMock.Object, _uowMock.Object);
    }

    [Fact]
    public async Task Handle_PendingOrder_CancelsSuccessfully()
    {
        var orderId = Guid.NewGuid();
        var order = new Order(Guid.NewGuid());
        typeof(Order).GetProperty(nameof(Order.Id))!.SetValue(order, orderId);

        _orderRepoMock.Setup(r => r.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var command = new CancelOrderCommand { OrderId = orderId };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.Equal(OrderStatus.Cancelled.ToString(), result.Status);
        _orderRepoMock.Verify(r => r.UpdateAsync(order, It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_OrderNotFound_ThrowsNotFoundException()
    {
        var orderId = Guid.NewGuid();
        _orderRepoMock.Setup(r => r.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var command = new CancelOrderCommand { OrderId = orderId };

        await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShippedOrder_ThrowsDomainException()
    {
        var orderId = Guid.NewGuid();
        var order = new Order(Guid.NewGuid());
        typeof(Order).GetProperty(nameof(Order.Id))!.SetValue(order, orderId);
        typeof(Order).GetProperty(nameof(Order.Status))!.SetValue(order, OrderStatus.Shipped);

        _orderRepoMock.Setup(r => r.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var command = new CancelOrderCommand { OrderId = orderId };

        await Assert.ThrowsAsync<DomainException>(
            () => _handler.Handle(command, CancellationToken.None));
    }
}
