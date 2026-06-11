using ECommercePoc.Application.Dtos;
using MediatR;

namespace ECommercePoc.Application.Commands.CreateProduct;

/// <summary>
/// Creates a new product in the catalog.
/// </summary>
public sealed class CreateProductCommand : IRequest<ProductDto>
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string Currency { get; init; } = "USD";
    public string Sku { get; init; } = string.Empty;
}
