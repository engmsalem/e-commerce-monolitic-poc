using System.Text.Json;
using Xunit;

namespace ECommercePoc.Api.Tests;

public class SwaggerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public SwaggerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Theory]
    [InlineData("/api/v1/orders/{id}", "get")]
    [InlineData("/api/v1/orders", "post")]
    [InlineData("/api/v1/orders/{id}/cancel", "post")]
    [InlineData("/api/v1/products", "post")]
    public async Task ProtectedOperations_RequireBearerScheme(string path, string method)
    {
        var operation = await GetOperationAsync(path, method);

        Assert.True(operation.TryGetProperty("security", out var security),
            $"{method.ToUpper()} {path} has no security requirement");

        var schemes = security.EnumerateArray()
            .SelectMany(requirement => requirement.EnumerateObject())
            .Select(scheme => scheme.Name);
        Assert.Contains("Bearer", schemes);
    }

    [Theory]
    [InlineData("/api/v1/auth/login", "post")]
    [InlineData("/api/v1/products", "get")]
    public async Task AnonymousOperations_HaveNoSecurityRequirement(string path, string method)
    {
        var operation = await GetOperationAsync(path, method);

        Assert.False(operation.TryGetProperty("security", out _),
            $"{method.ToUpper()} {path} should not require authentication");
    }

    [Fact]
    public async Task Paths_HaveApiVersionSubstituted()
    {
        using var document = await GetSwaggerDocumentAsync();

        var paths = document.RootElement.GetProperty("paths")
            .EnumerateObject().Select(p => p.Name).ToList();

        Assert.NotEmpty(paths);
        Assert.All(paths, path => Assert.DoesNotContain("{version}", path));
    }

    private async Task<JsonElement> GetOperationAsync(string path, string method)
    {
        using var document = await GetSwaggerDocumentAsync();

        Assert.True(document.RootElement.GetProperty("paths").TryGetProperty(path, out var pathItem),
            $"Path {path} not found in swagger.json");
        Assert.True(pathItem.TryGetProperty(method, out var operation),
            $"{method.ToUpper()} {path} not found in swagger.json");

        return operation.Clone();
    }

    private async Task<JsonDocument> GetSwaggerDocumentAsync()
    {
        var response = await _client.GetAsync("/swagger/v1/swagger.json");
        response.EnsureSuccessStatusCode();
        return JsonDocument.Parse(await response.Content.ReadAsStringAsync());
    }
}
