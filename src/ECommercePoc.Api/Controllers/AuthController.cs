using Asp.Versioning;
using ECommercePoc.Application.Auth;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ECommercePoc.Api.Controllers;

/// <summary>
/// Authentication endpoint — issues JWT tokens.
/// This is intentionally unauthenticated (the entry point for getting a token).
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Authenticates with username/password and returns a JWT Bearer token.
    /// POC credentials: admin / admin123
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login(
        [FromBody] LoginCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}
