namespace ECommercePoc.Application.Interfaces;

/// <summary>
/// Transactional boundary for the application layer.
/// Implemented in Infrastructure via EF Core's SaveChangesAsync.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
