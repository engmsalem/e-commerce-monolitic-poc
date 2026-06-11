using ECommercePoc.Domain.ValueObjects;

namespace ECommercePoc.Domain.Entities;

/// <summary>
/// A line item within an Order. Stores a snapshot of the product name and
/// price at time of ordering — the order should not change if the product
/// catalog is later updated.
/// </summary>
public class OrderItem
{
    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; }
    public int Quantity { get; private set; }
    public Money UnitPrice { get; private set; }

#pragma warning disable CS8618 // EF Core parameterless constructor
    private OrderItem() { }
#pragma warning restore CS8618

    public OrderItem(Guid productId, string productName, int quantity, Money unitPrice)
    {
        Id = Guid.NewGuid();
        ProductId = productId;
        ProductName = productName ?? throw new ArgumentNullException(nameof(productName));
        Quantity = quantity;
        UnitPrice = unitPrice ?? throw new ArgumentNullException(nameof(unitPrice));
    }

    public void UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(newQuantity));
        Quantity = newQuantity;
    }
}
