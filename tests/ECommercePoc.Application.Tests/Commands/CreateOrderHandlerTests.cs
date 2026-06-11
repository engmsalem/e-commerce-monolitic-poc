using ECommercePoc.Application.Commands.CreateOrder;
using ECommercePoc.Application.Exceptions;
using ECommercePoc.Application.Interfaces;
using ECommercePoc.Domain.Entities;
using ECommercePoc.Domain.ValueObjects;
using Moq;
using Xunit;

namespace ECommercePoc.Application.Tests.Commands;

public class CreateOrderHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepoMock = new();
    private readonly Mock<IProductRepository> _productRepoMock = new();
    private readonly Mock<ICustomerRepository> _customerRepoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly CreateOrderHandler _handler;

    public CreateOrderHandlerTests()
    {
        _handler = new CreateOrderHandler(
            _orderRepoMock.Object,
            _productRepoMock.Object,
            _customerRepoMock.Object,
            _uowMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_CreatesOrder()
    {
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var customer = new Customer("Jane", "Smith", "jane@example.com");
        typeof(Customer).GetProperty(nameof(Customer.Id))!.SetValue(customer, customerId);

        var product = new Product("Keyboard", "Mech", new Money(99.99m, "USD"), "KB-01");
        typeof(Product).GetProperty(nameof(Product.Id))!.SetValue(product, productId);

        _customerRepoMock.Setup(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);
        _productRepoMock.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var command = new CreateOrderCommand
        {
            CustomerId = customerId,
            Items = [new OrderLineItemRequest { ProductId = productId, Quantity = 2 }]
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(customerId, result.CustomerId);
        Assert.Single(result.Items);
        Assert.Equal(2, result.Items[0].Quantity);
        _orderRepoMock.Verify(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CustomerNotFound_ThrowsNotFoundException()
    {
        var customerId = Guid.NewGuid();
        _customerRepoMock.Setup(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        var command = new CreateOrderCommand { CustomerId = customerId, Items = [] };

        await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ProductNotFound_ThrowsNotFoundException()
    {
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var customer = new Customer("Jane", "Smith", "jane@example.com");
        typeof(Customer).GetProperty(nameof(Customer.Id))!.SetValue(customer, customerId);

        _customerRepoMock.Setup(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);
        _productRepoMock.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var command = new CreateOrderCommand
        {
            CustomerId = customerId,
            Items = [new OrderLineItemRequest { ProductId = productId, Quantity = 1 }]
        };

        await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(command, CancellationToken.None));
    }
}
