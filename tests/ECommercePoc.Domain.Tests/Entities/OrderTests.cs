using ECommercePoc.Domain.Entities;
using ECommercePoc.Domain.Enums;
using ECommercePoc.Domain.Exceptions;
using ECommercePoc.Domain.ValueObjects;
using Xunit;

namespace ECommercePoc.Domain.Tests.Entities;

public class OrderTests
{
    [Fact]
    public void Cancel_PendingOrder_SetsStatusToCancelled()
    {
        var order = new Order(Guid.NewGuid());
        order.Cancel();

        Assert.Equal(OrderStatus.Cancelled, order.Status);
    }

    [Fact]
    public void Cancel_ShippedOrder_ThrowsDomainException()
    {
        var order = CreateOrderWithStatus(OrderStatus.Shipped);

        var ex = Assert.Throws<DomainException>(() => order.Cancel());
        Assert.Contains("shipped", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Cancel_AlreadyCancelledOrder_ThrowsDomainException()
    {
        var order = CreateOrderWithStatus(OrderStatus.Cancelled);

        Assert.Throws<DomainException>(() => order.Cancel());
    }

    [Fact]
    public void AddItem_ValidProduct_AddsItemToOrder()
    {
        var order = new Order(Guid.NewGuid());
        var product = new Product("Keyboard", "Mech", new Money(99.99m, "USD"), "KB-01");

        order.AddItem(product, 2);

        Assert.Single(order.Items);
        Assert.Equal(2, order.Items.First().Quantity);
        Assert.Equal(199.98m, order.Total.Amount);
    }

    [Fact]
    public void AddItem_ZeroQuantity_ThrowsDomainException()
    {
        var order = new Order(Guid.NewGuid());
        var product = new Product("Mouse", "Wireless", new Money(29.99m, "USD"), "WM-01");

        Assert.Throws<DomainException>(() => order.AddItem(product, 0));
    }

    [Fact]
    public void AddItem_NegativeQuantity_ThrowsDomainException()
    {
        var order = new Order(Guid.NewGuid());
        var product = new Product("Mouse", "Wireless", new Money(29.99m, "USD"), "WM-01");

        Assert.Throws<DomainException>(() => order.AddItem(product, -1));
    }

    [Fact]
    public void AddItem_DuplicateProduct_UpdatesQuantity()
    {
        var order = new Order(Guid.NewGuid());
        var product = new Product("USB Cable", "1m", new Money(9.99m, "USD"), "CBL-01");

        order.AddItem(product, 1);
        order.AddItem(product, 2);

        Assert.Single(order.Items);
        Assert.Equal(3, order.Items.First().Quantity);
    }

    [Fact]
    public void Total_EmptyOrder_ReturnsZero()
    {
        var order = new Order(Guid.NewGuid());
        Assert.Equal(0, order.Total.Amount);
    }

    private static Order CreateOrderWithStatus(OrderStatus status)
    {
        var order = new Order(Guid.NewGuid());
        typeof(Order).GetProperty(nameof(Order.Status))!
            .SetValue(order, status);
        return order;
    }
}
