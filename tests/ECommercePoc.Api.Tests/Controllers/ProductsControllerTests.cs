using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace ECommercePoc.Api.Tests.Controllers;

public class ProductsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ProductsControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateProduct_DuplicateSku_Returns409ProblemDetails()
    {
        await AuthenticateAsync();
        // The in-memory test database is shared across test classes, so use a unique SKU.
        var sku = $"DUP-{Guid.NewGuid():N}"[..20];
        var product = new { Name = "Duplicate", Description = "Test", Price = 10m, Currency = "USD", Sku = sku };

        var first = await _client.PostAsJsonAsync("/api/v1/products", product);
        var second = await _client.PostAsJsonAsync("/api/v1/products", product);

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
        Assert.Equal("application/problem+json", second.Content.Headers.ContentType?.MediaType);

        var problem = await second.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal(409, problem!.Status);
        Assert.Equal("Conflict", problem.Title);
        Assert.Equal($"A product with SKU '{sku}' already exists.", problem.Detail);
    }

    private async Task AuthenticateAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new { Username = "admin", Password = "admin123" });
        var login = await response.Content.ReadFromJsonAsync<LoginResult>();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login!.Token);
    }

    private sealed class LoginResult
    {
        public string Token { get; set; } = string.Empty;
    }
}
