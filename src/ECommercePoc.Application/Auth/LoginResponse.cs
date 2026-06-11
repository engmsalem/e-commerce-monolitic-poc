namespace ECommercePoc.Application.Auth;

/// <summary>
/// DTO returned by the login endpoint.
/// </summary>
public sealed class LoginResponse
{
    public string Token { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
}
