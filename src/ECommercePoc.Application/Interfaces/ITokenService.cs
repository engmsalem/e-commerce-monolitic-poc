namespace ECommercePoc.Application.Interfaces;

/// <summary>
/// Abstraction for JWT token generation. Implemented in Infrastructure.
/// Application layer depends on this contract, not on JWT implementation details.
/// </summary>
public interface ITokenService
{
    string GenerateToken(string username);
}
