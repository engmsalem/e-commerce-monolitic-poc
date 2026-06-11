namespace ECommercePoc.Application.Dtos;

/// <summary>
/// Read-side projection of Product — decoupled from domain entity.
/// </summary>
public sealed class ProductDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string Currency { get; init; } = "USD";
    public string Sku { get; init; } = string.Empty;
}
