using ECommercePoc.Application.Dtos;
using MediatR;

namespace ECommercePoc.Application.Queries.GetProducts;

/// <summary>
/// Retrieves all products from the catalog.
/// </summary>
public sealed class GetProductsQuery : IRequest<IReadOnlyList<ProductDto>>;
