using ECommercePoc.Domain.Entities;

namespace ECommercePoc.Application.Interfaces;

/// <summary>
/// Repository contract for Product aggregate. Implementation lives in Infrastructure.
/// </summary>
public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Product product, CancellationToken cancellationToken = default);
}
