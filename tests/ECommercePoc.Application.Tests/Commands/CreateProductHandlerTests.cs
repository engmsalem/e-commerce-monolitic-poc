using ECommercePoc.Application.Commands.CreateProduct;
using ECommercePoc.Application.Exceptions;
using ECommercePoc.Application.Interfaces;
using ECommercePoc.Domain.Entities;
using Moq;
using Xunit;

namespace ECommercePoc.Application.Tests.Commands;

public class CreateProductHandlerTests
{
    private readonly Mock<IProductRepository> _productRepoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly CreateProductHandler _handler;

    public CreateProductHandlerTests()
    {
        _handler = new CreateProductHandler(_productRepoMock.Object, _uowMock.Object);
    }

    private static CreateProductCommand Command(string sku = "KB-01") => new()
    {
        Name = "Keyboard",
        Description = "Mechanical",
        Price = 99.99m,
        Currency = "USD",
        Sku = sku
    };

    [Fact]
    public async Task Handle_NewSku_CreatesProduct()
    {
        _productRepoMock.Setup(r => r.ExistsBySkuAsync("KB-01", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _handler.Handle(Command(), CancellationToken.None);

        Assert.Equal("KB-01", result.Sku);
        Assert.Equal(99.99m, result.Price);
        _productRepoMock.Verify(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateSku_ThrowsConflictAndDoesNotSave()
    {
        _productRepoMock.Setup(r => r.ExistsBySkuAsync("KB-01", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var ex = await Assert.ThrowsAsync<ConflictException>(
            () => _handler.Handle(Command(), CancellationToken.None));

        Assert.Equal("A product with SKU 'KB-01' already exists.", ex.Message);
        _productRepoMock.Verify(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
