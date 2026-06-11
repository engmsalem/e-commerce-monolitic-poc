using ECommercePoc.Domain.ValueObjects;
using Xunit;

namespace ECommercePoc.Domain.Tests.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void Equals_SameAmountAndCurrency_ReturnsTrue()
    {
        var m1 = new Money(10.00m, "USD");
        var m2 = new Money(10.00m, "USD");

        Assert.True(m1.Equals(m2));
        Assert.True(m1 == m2);
    }

    [Fact]
    public void Equals_DifferentAmount_ReturnsFalse()
    {
        var m1 = new Money(10.00m, "USD");
        var m2 = new Money(20.00m, "USD");

        Assert.False(m1.Equals(m2));
    }

    [Fact]
    public void Equals_DifferentCurrency_ReturnsFalse()
    {
        var m1 = new Money(10.00m, "USD");
        var m2 = new Money(10.00m, "EUR");

        Assert.False(m1.Equals(m2));
    }

    [Fact]
    public void Constructor_NegativeAmount_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Money(-1m, "USD"));
    }

    [Fact]
    public void Constructor_EmptyCurrency_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Money(10m, ""));
    }

    [Fact]
    public void Add_SameCurrency_ReturnsSum()
    {
        var a = new Money(10m, "USD");
        var b = new Money(15m, "USD");

        var result = a + b;

        Assert.Equal(25m, result.Amount);
        Assert.Equal("USD", result.Currency);
    }

    [Fact]
    public void Add_DifferentCurrency_Throws()
    {
        var a = new Money(10m, "USD");
        var b = new Money(10m, "EUR");

        Assert.Throws<InvalidOperationException>(() => a + b);
    }

    [Fact]
    public void Multiply_ByQuantity_ReturnsCorrectAmount()
    {
        var price = new Money(29.99m, "USD");
        var result = price * 3;

        Assert.Equal(89.97m, result.Amount);
    }
}
