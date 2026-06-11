using ECommercePoc.Application.Interfaces;
using ECommercePoc.Domain.Exceptions;
using MediatR;

namespace ECommercePoc.Application.Auth;

/// <summary>
/// Validates hardcoded credentials (POC) and returns a JWT via ITokenService.
/// </summary>
public sealed class LoginHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly ITokenService _tokenService;

    // POC credentials — in production, validate against a user store
    private const string ValidUsername = "admin";
    private const string ValidPassword = "admin123";

    public LoginHandler(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }

    public Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        if (request.Username != ValidUsername || request.Password != ValidPassword)
            throw new DomainException("Invalid username or password.");

        var token = _tokenService.GenerateToken(request.Username);

        return Task.FromResult(new LoginResponse
        {
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        });
    }
}
