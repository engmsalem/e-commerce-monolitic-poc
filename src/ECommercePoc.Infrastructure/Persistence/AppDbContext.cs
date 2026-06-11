using System.Reflection;
using ECommercePoc.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommercePoc.Infrastructure.Persistence;

/// <summary>
/// EF Core database context. Applies entity configurations from this assembly.
/// </summary>
public class AppDbContext : DbContext
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Customer> Customers => Set<Customer>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
