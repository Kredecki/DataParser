using DataParser.Infrastructure.Commands.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using DataParser.Shared.DTOs.Authorization;

namespace DataParser.API.Controllers;

[ApiController]
[Route("api/v1")]
public class AuthorizationController(IMediator sender) : Controller
{
	/// <summary>
	/// User SignIn endpoint.
	/// </summary>
	/// <remarks>
	///     POST /api/v1/SignIn \
	///     { \
	///         "login": "LOGIN" \
	///         "password": "USER PASSWORD" \
	///     }
	/// </remarks>
	/// <response code="200">Returns authenticated user's login and jwt token</response>
	/// <response code="400">Credentials are incorrect, auth failed</response>
	[AllowAnonymous]
	[HttpPost("SignIn")]
	[Produces("application/json")]
	[ProducesResponseType(200, Type = typeof(SignInResponseDto))]
	[ProducesResponseType(400, Type = typeof(ProblemDetails))]
	public async Task<IActionResult> SignIn(SignInRequestDto dto, CancellationToken cancellationToken = default)
	{
		SignInResponseDto? response = await sender.Send(new SignInCommand(dto), cancellationToken);

		if (response is not null)
			return Ok(response);

		return BadRequest();
	}
}
