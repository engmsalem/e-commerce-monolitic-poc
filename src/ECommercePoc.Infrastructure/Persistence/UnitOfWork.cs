using ECommercePoc.Application.Interfaces;

namespace ECommercePoc.Infrastructure.Persistence;

/// <summary>
/// Transactional unit of work — delegates to EF Core's SaveChangesAsync.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
