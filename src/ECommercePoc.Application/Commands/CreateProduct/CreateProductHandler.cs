using ECommercePoc.Application.Dtos;
using ECommercePoc.Application.Exceptions;
using ECommercePoc.Application.Interfaces;
using ECommercePoc.Domain.Entities;
using ECommercePoc.Domain.ValueObjects;
using MediatR;

namespace ECommercePoc.Application.Commands.CreateProduct;

public sealed class CreateProductHandler : IRequestHandler<CreateProductCommand, ProductDto>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateProductHandler(IProductRepository productRepository, IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        // SKU has a unique index; check first so a duplicate is a 409, not a database error (500).
        if (await _productRepository.ExistsBySkuAsync(request.Sku, cancellationToken))
            throw new ConflictException($"A product with SKU '{request.Sku}' already exists.");

        var price = new Money(request.Price, request.Currency);
        var product = new Product(request.Name, request.Description, price, request.Sku);

        await _productRepository.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(product);
    }

    private static ProductDto MapToDto(Product product) => new()
    {
        Id = product.Id,
        Name = product.Name,
        Description = product.Description,
        Price = product.Price.Amount,
        Currency = product.Price.Currency,
        Sku = product.Sku
    };
}
