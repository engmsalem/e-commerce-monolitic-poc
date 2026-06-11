using ECommercePoc.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ECommercePoc.Api.Tests;

/// <summary>
/// Custom WebApplicationFactory that replaces SQLite with an in-memory database
/// for isolated, repeatable integration tests.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the SQLite DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor is not null)
                services.Remove(descriptor);

            // Replace with in-memory
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase("ECommercePocTestDb"));
        });
    }
}
