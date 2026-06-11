using ECommercePoc.Domain.Entities;
using ECommercePoc.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommercePoc.Infrastructure.Persistence;

/// <summary>
/// Seeds the database with sample data for the POC. Runs on startup in Development.
/// </summary>
public static class DataSeeder
{
    public static async Task SeedAsync(AppDbContext context, ILogger logger)
    {
        if (await context.Products.AnyAsync())
        {
            logger.LogInformation("Database already seeded — skipping.");
            return;
        }

        var products = new[]
        {
            new Product("Wireless Mouse", "Ergonomic wireless mouse with USB-C charging",
                new Money(29.99m, "USD"), "WM-001"),
            new Product("Mechanical Keyboard", "RGB mechanical keyboard with Cherry MX switches",
                new Money(89.99m, "USD"), "KB-MECH-002"),
            new Product("USB-C Hub", "7-in-1 USB-C hub with HDMI and SD card reader",
                new Money(49.99m, "USD"), "HUB-003"),
            new Product("27\" 4K Monitor", "27-inch 4K IPS monitor with HDR",
                new Money(449.99m, "USD"), "MON-4K-004"),
            new Product("Webcam Pro", "1080p webcam with noise-cancelling microphone",
                new Money(79.99m, "USD"), "WC-PRO-005")
        };

        var customer = new Customer("Jane", "Smith", "jane.smith@example.com");

        context.Products.AddRange(products);
        context.Customers.Add(customer);

        await context.SaveChangesAsync();

        logger.LogInformation(
            "Seeded {ProductCount} products and 1 customer (ID: {CustomerId})",
            products.Length, customer.Id);
    }
}
