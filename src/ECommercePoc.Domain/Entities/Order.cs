using ECommercePoc.Domain.Enums;
using ECommercePoc.Domain.Exceptions;
using ECommercePoc.Domain.ValueObjects;

namespace ECommercePoc.Domain.Entities;

/// <summary>
/// Order aggregate root — the central entity of the e-commerce domain.
/// Enforces business invariants: cannot cancel a shipped order,
/// items must be valid, total is derived from items.
/// </summary>
public class Order
{
    private readonly List<OrderItem> _items = new();

    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime CreatedDate { get; private set; }
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
    public Money Total => CalculateTotal();

#pragma warning disable CS8618 // EF Core parameterless constructor
    private Order() { }
#pragma warning restore CS8618

    public Order(Guid customerId)
    {
        Id = Guid.NewGuid();
        CustomerId = customerId;
        Status = OrderStatus.Pending;
        CreatedDate = DateTime.UtcNow;
    }

    public void AddItem(Product product, int quantity)
    {
        if (product is null)
            throw new ArgumentNullException(nameof(product));
        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than zero.");

        var existingItem = _items.FirstOrDefault(i => i.ProductId == product.Id);

        if (existingItem is not null)
        {
            existingItem.UpdateQuantity(existingItem.Quantity + quantity);
        }
        else
        {
            _items.Add(new OrderItem(product.Id, product.Name, quantity, product.Price));
        }
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Shipped)
            throw new DomainException("Cannot cancel an order that has already been shipped.");
        if (Status == OrderStatus.Cancelled)
            throw new DomainException("Order is already cancelled.");

        Status = OrderStatus.Cancelled;
    }

    private Money CalculateTotal()
    {
        if (_items.Count == 0)
            return Money.Zero();

        var total = _items
            .Select(item => item.UnitPrice * item.Quantity)
            .Aggregate((a, b) => a + b);

        return total;
    }
}
