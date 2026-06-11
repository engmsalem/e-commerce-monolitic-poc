using ECommercePoc.Application.Commands.CreateOrder;
using ECommercePoc.Application.Validators;
using Xunit;

namespace ECommercePoc.Application.Tests.Validators;

public class CreateOrderCommandValidatorTests
{
    private readonly CreateOrderCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_Passes()
    {
        var command = new CreateOrderCommand
        {
            CustomerId = Guid.NewGuid(),
            Items = [new OrderLineItemRequest { ProductId = Guid.NewGuid(), Quantity = 1 }]
        };

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyCustomerId_Fails()
    {
        var command = new CreateOrderCommand
        {
            CustomerId = Guid.Empty,
            Items = [new OrderLineItemRequest { ProductId = Guid.NewGuid(), Quantity = 1 }]
        };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "CustomerId");
    }

    [Fact]
    public void Validate_NoItems_Fails()
    {
        var command = new CreateOrderCommand
        {
            CustomerId = Guid.NewGuid(),
            Items = []
        };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Items");
    }

    [Fact]
    public void Validate_ZeroQuantity_Fails()
    {
        var command = new CreateOrderCommand
        {
            CustomerId = Guid.NewGuid(),
            Items = [new OrderLineItemRequest { ProductId = Guid.NewGuid(), Quantity = 0 }]
        };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Items[0].Quantity");
    }
}
