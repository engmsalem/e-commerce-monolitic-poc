using ECommercePoc.Application.Auth;
using ECommercePoc.Application.Interfaces;
using ECommercePoc.Domain.Exceptions;
using Moq;
using Xunit;

namespace ECommercePoc.Application.Tests.Auth;

public class LoginHandlerTests
{
    private readonly Mock<ITokenService> _tokenServiceMock = new();
    private readonly LoginHandler _handler;

    public LoginHandlerTests()
    {
        _tokenServiceMock
            .Setup(t => t.GenerateToken(It.IsAny<string>()))
            .Returns("fake-jwt-token");

        _handler = new LoginHandler(_tokenServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsToken()
    {
        var command = new LoginCommand
        {
            Username = "admin",
            Password = "admin123"
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("fake-jwt-token", result.Token);
        Assert.True(result.ExpiresAt > DateTime.UtcNow);
    }

    [Fact]
    public async Task Handle_InvalidPassword_ThrowsDomainException()
    {
        var command = new LoginCommand
        {
            Username = "admin",
            Password = "wrong"
        };

        await Assert.ThrowsAsync<DomainException>(
            () => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_InvalidUsername_ThrowsDomainException()
    {
        var command = new LoginCommand
        {
            Username = "hacker",
            Password = "admin123"
        };

        await Assert.ThrowsAsync<DomainException>(
            () => _handler.Handle(command, CancellationToken.None));
    }
}
