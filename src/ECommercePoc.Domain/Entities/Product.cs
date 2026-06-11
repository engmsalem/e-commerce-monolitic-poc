using ECommercePoc.Domain.ValueObjects;

namespace ECommercePoc.Domain.Entities;

/// <summary>
/// Product aggregate root — what customers can order.
/// </summary>
public class Product
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public Money Price { get; private set; }
    public string Sku { get; private set; }

#pragma warning disable CS8618 // EF Core parameterless constructor
    private Product() { }
#pragma warning restore CS8618

#pragma warning disable CS8618
    public Product(string name, string description, Money price, string sku)
#pragma warning restore CS8618
    {
        Id = Guid.NewGuid();
        SetName(name);
        SetDescription(description);
        SetPrice(price);
        SetSku(sku);
    }

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name is required.", nameof(name));
        Name = name;
    }

    public void SetDescription(string description)
    {
        Description = description ?? string.Empty;
    }

    public void SetPrice(Money price)
    {
        Price = price ?? throw new ArgumentNullException(nameof(price));
    }

    public void SetSku(string sku)
    {
        if (string.IsNullOrWhiteSpace(sku))
            throw new ArgumentException("SKU is required.", nameof(sku));
        Sku = sku;
    }
}
