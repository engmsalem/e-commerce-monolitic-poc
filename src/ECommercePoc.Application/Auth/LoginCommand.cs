using MediatR;

namespace ECommercePoc.Application.Auth;

/// <summary>
/// Authenticates a user and returns a JWT token.
/// </summary>
public sealed class LoginCommand : IRequest<LoginResponse>
{
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
